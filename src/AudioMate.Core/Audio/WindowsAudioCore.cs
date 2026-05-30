using System.Diagnostics;
using AudioMate.Core.Configuration;
using AudioMate.Core.Music;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AudioMate.Core.Audio;

/// <summary>
/// Applies AudioMate ducking decisions to Windows render audio sessions.
/// </summary>
public sealed class WindowsAudioCore : IAudioCore, IAudioSessionScanner, IMicrophoneLevelScanner, IDisposable
{
    private readonly Dictionary<int, float> _originalVolumes = [];
    private readonly object _gate = new();
    private DuckingStateMachine _stateMachine;
    private AudioMateSettings _settings;
    private MusicTargetMatcher _matcher;
    private Timer? _restoreTimer;
    private bool _disposed;

    public WindowsAudioCore(AudioMateSettings settings)
    {
        _settings = settings.Normalize();
        _stateMachine = new DuckingStateMachine(_settings);
        _matcher = new MusicTargetMatcher(_settings);
    }

    public DuckingSnapshot CurrentState => _stateMachine.Snapshot;

    public void ApplySettings(AudioMateSettings settings)
    {
        lock (_gate)
        {
            RestoreMusicSessions();
            _settings = settings.Normalize();
            _stateMachine.ApplySettings(_settings);
            _matcher = new MusicTargetMatcher(_settings);
            ApplyVolumeState();
        }
    }

    public DuckingSnapshot EnterTrigger(AudioTriggerKind trigger)
    {
        lock (_gate)
        {
            var snapshot = _stateMachine.EnterTrigger(trigger);
            ApplyVolumeState();
            return snapshot;
        }
    }

    public DuckingSnapshot ExitTrigger(AudioTriggerKind trigger)
    {
        lock (_gate)
        {
            var snapshot = _stateMachine.ExitTrigger(trigger);
            ApplyVolumeState();
            return snapshot;
        }
    }

    public IReadOnlyList<AudioSessionInfo> ScanRenderSessions()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var sessions = device.AudioSessionManager.Sessions;
        var results = new List<AudioSessionInfo>();

        for (var index = 0; index < sessions.Count; index++)
        {
            var session = sessions[index];
            var processName = ResolveProcessName((int)session.GetProcessID);
            results.Add(new AudioSessionInfo(
                (int)session.GetProcessID,
                processName,
                session.SimpleAudioVolume.Volume,
                session.AudioMeterInformation.MasterPeakValue,
                session.State == AudioSessionState.AudioSessionStateActive));
        }

        return results;
    }

    public float GetDefaultMicrophonePeakValue()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        return device.AudioMeterInformation.MasterPeakValue;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        RestoreMusicSessions();
        _restoreTimer?.Dispose();
        _disposed = true;
    }

    private void ApplyVolumeState()
    {
        if (_stateMachine.Snapshot.IsDucking)
        {
            _restoreTimer?.Dispose();
            _restoreTimer = null;
            DuckMusicSessions();
            return;
        }

        ScheduleRestore();
    }

    private void ScheduleRestore()
    {
        _restoreTimer?.Dispose();
        var delay = _stateMachine.Snapshot.RestoreDelay;
        _restoreTimer = new Timer(
            _ =>
            {
                lock (_gate)
                {
                    if (!_stateMachine.Snapshot.IsDucking)
                    {
                        RestoreMusicSessions();
                    }
                }
            },
            null,
            delay,
            Timeout.InfiniteTimeSpan);
    }

    private void DuckMusicSessions()
    {
        var duckVolume = _settings.DuckVolumePercent / 100f;
        ChangeMusicSessionVolumes(
            (session, currentVolume) =>
            {
                var processId = (int)session.GetProcessID;
                _originalVolumes.TryAdd(processId, currentVolume);
                return duckVolume;
            },
            _settings.Fade.FadeOutMilliseconds);
    }

    private void RestoreMusicSessions()
    {
        ChangeMusicSessionVolumes(
            (session, currentVolume) =>
            {
                var processId = (int)session.GetProcessID;
                return _originalVolumes.TryGetValue(processId, out var originalVolume)
                    ? originalVolume
                    : currentVolume;
            },
            _settings.Fade.FadeInMilliseconds);

        _originalVolumes.Clear();
    }

    private void ChangeMusicSessionVolumes(
        Func<AudioSessionControl, float, float> resolveTargetVolume,
        int durationMilliseconds)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var sessions = device.AudioSessionManager.Sessions;
        var targets = new List<(AudioSessionControl Session, float Start, float End)>();

        for (var index = 0; index < sessions.Count; index++)
        {
            var session = sessions[index];
            if (_matcher.IsMusicTarget(ResolveProcessName((int)session.GetProcessID)))
            {
                var start = session.SimpleAudioVolume.Volume;
                var end = Math.Clamp(resolveTargetVolume(session, start), 0, 1);
                targets.Add((session, start, end));
            }
        }

        if (targets.Count == 0)
        {
            return;
        }

        if (!_settings.Fade.IsEnabled || durationMilliseconds <= 0)
        {
            foreach (var target in targets)
            {
                target.Session.SimpleAudioVolume.Volume = target.End;
            }

            return;
        }

        var steps = VolumeFadePlanner.BuildProgressSteps(durationMilliseconds);
        var delay = Math.Max(1, durationMilliseconds / steps.Count);

        foreach (var progress in steps)
        {
            foreach (var target in targets)
            {
                target.Session.SimpleAudioVolume.Volume = VolumeFadePlanner.Interpolate(target.Start, target.End, progress);
            }

            Thread.Sleep(delay);
        }
    }

    private static string ResolveProcessName(int processId)
    {
        if (processId <= 0)
        {
            return "system";
        }

        try
        {
            using var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch (ArgumentException)
        {
            return $"pid-{processId}";
        }
        catch (InvalidOperationException)
        {
            return $"pid-{processId}";
        }
    }
}
