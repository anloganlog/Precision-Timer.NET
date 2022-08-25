
# PrecisionTimer .NET

[![Nuget](https://img.shields.io/nuget/v/PrecisionTimer.NET)](https://www.nuget.org/packages/PrecisionTimer.NET/)

This timer is as precise as the platform you are running it on down to 1 millisecond.
 
`PrecisionTimer.NET` won't randomly dispose itself, You don't need to keep a special reference to it. 

If you intend to use a lot of timers or you want to use `PrecisionTimer.NET` to repeat a long running task, consider using [`Timer Sink.NET`](https://www.nuget.org/packages/TimerSink.NET) as it uses `PrecisionTimer.NET` internally.

# Usage
```cs
using PrecisionTiming;
```
Using Precision Timer is as simple as using any other Windows Timer.

```cs
public readonly PrecisionTimer MyTimer = new();
MyTimer.SetInterval(SomeAction, Interval);
```

If you use `SetInterval` to set only `Action` and `Interval` the timer will automatically start with the most common defaults, but it has several optional parameters you can change

![](https://i.imgur.com/HdHkL17.png)

For example, If you don't want the timer to start automatically then use the following

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

# Manual Setup

You can manually configure the `Timer` and then `Start` it yourself instead of using `SetInterval`


## SetAction

Sets the `Action` that will be triggered by the TimerCallback when the `Period` has elapsed
```cs
MyTimer.SetAction(Action);
```

## SetInterval

Sets the Interval between `Actions` in `Milliseconds`.
```cs
MyTimer.SetInterval(int);
```
```cs
MyTimer.GetInterval();
```

## SetResolution

Set the `Resolution` of the Timer
```cs
MyTimer.SetResolution(int);
```
```cs
MyTimer.GetResolution();
```

The resolution is in milliseconds, The default resolution for `SetInterval(Action)` is 0

The `Resolution` increases with smaller values.

A resolution of 0 indicates periodic events should occur with the greatest possible accuracy. 

To reduce system overhead, however, you should use the maximum value appropriate for your application.

The normal Resolution of a .Net Timer is around 12-15ms



## SetEventArgs

Set `EventArgs` of the Timer

```cs
MyTimer.SetEventArgs(EventArgs);
```
```cs
MyTimer.GetEventArgs();
```


## SetPeriodic

Set the Periodic/OneShot Mode of the <see cref="PrecisionTimer"/>

```cs
MyTimer.SetPeriodic(bool);
```
```cs
MyTimer.GetPeriodic();
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

Consider using [`Timer Sink.NET`](https://www.nuget.org/packages/TimerSink.NET) instead of using PrecisionTimer.NET directly
