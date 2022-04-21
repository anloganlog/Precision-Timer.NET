# Precision Timer.NET

[![Nuget](https://img.shields.io/nuget/v/PrecisionTimer.NET)](https://www.nuget.org/packages/PrecisionTimer.NET/)

This timer is as precise as the platform you are running it on down to 1 millisecond.
 
PrecisionTimer.NET won't randomly dispose itself, You don't need to keep a special reference to it. 
 
If you intend to use a lot of timers, consider using [Timer Sink.NET](https://github.com/HypsyNZ/Timer-Sink.NET)
 
# Usage

Using Precision Timer is as simple as using any other Windows Timer.

```cs
public readonly PrecisionTimer MyTimer = new();
MyTimer.SetInterval(SomeAction, Interval);
```

If you use `SetInterval` to set `Action` and provide `Interval` the timer will automatically start with the default settings

If you don't want the timer to start automatically then use the following

```cs
MyTimer.SetInterval(SomeAction, Interval, false);
```

## Basics

Start the Timer
```cs
MyTimer.Start();
```

Check if the Timer is Running
```cs
MyTimer.IsRunning();
```

Stop the Timer
```cs
MyTimer.Stop();
```

Dispose the Timer
```cs
MyTimer.Dispose();
```

## SetAction

Sets the `Action` that will be triggered by the TimerCallback when the `Period` has elapsed
```cs
MyTimer.SetAction(Action);
```

## SetPeriod

Sets the Interval between `Actions` in `Milliseconds`.
```cs
MyTimer.SetPeriod(int);
```

## SetResolution

Set the `Resolution` of the Timer
```cs
MyTimer.SetResolution(int);
```

The resolution is in milliseconds. 

The `Resolution` increases with smaller values.

A resolution of 0 indicates periodic events should occur with the greatest possible accuracy. 

To reduce system overhead, however, you should use the maximum value appropriate for your application.

The normal Resolution of a .Net Timer is around 12-15ms

## SetAutoReset

Set if the `Action` should reset (repeat)
```cs
MyTimer.SetAutoReset(bool);
```

`True` if the `PrecisionTimer` should raise the `Ticks` Event each time the interval elapses. (Periodic)

`False` if the `PrecisionTimer` should raise the event only once AFTER the first time the interval elapses. (One-Shot)


# Timing Settings

Global Multimedia Timer settings that affect your application

[Set the Applications Minimum Resolution](https://docs.microsoft.com/en-us/windows/win32/api/timeapi/nf-timeapi-timebeginperiod)
```cs
TimingSettings.SetMinimumTimerResolution(0);
```

[Clear the Applications Minimum Resolution](https://docs.microsoft.com/en-us/windows/win32/api/timeapi/nf-timeapi-timeendperiod)
```cs
TimingSettings.ClearMinimumTimerResolution(0);
```

## Multimedia Timer

Precision Timer.NET is a Multimedia Timer.

You can read more about Multimedia Timers on [MSDN](https://docs.microsoft.com/en-us/windows/win32/multimedia/multimedia-timer-reference)

# Tips

If you found this useful pleases consider leaving a tip

- [x] BTC: 1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm
- [x] ETH: 0x50740d132481be4721b1742670031baee3655ec2
- [x] DOGE: DS6orKQwdK4sBTmwoS9NVqvWCKA5ksGPfa
- [x] LTC: Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES