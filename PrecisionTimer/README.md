### Version 2.2.4.5
- [x] Fix Minimum Resolution (0 = use best available resolution)

### Version 2.2.4.4
- [x] Fix out of range exception

### Version 2.2.4.3
- [x] Fixes a bug that was causing event duplication
- [x] Throw on out of range exception
- [x] Improve Test

### Version 2.2.4.1
- [x] Documentation

### Version 2.2.4
- [x] Add Example
- [x] Expose `Tick` Event
- [x] Expose `Stopped` Event
- [x] Expose `Started` Event
- [x] `Dispose` is really `Destroy`

### Version 2.2.3
- [x] Nullable

### Version 2.2.2
- [x] `SetAction`
- [x] `GetPeriod`
- [x] `SetPeriod` - Set Before Start
- [x] `GetResolution`
- [x] `SetResolution`  - Set Before Start
- [x] `GetPeriodic`
- [x] `SetPeriodic` - Set Before Start
- [x] `GetEventArg`
- [x] `SetEventArgs` - Set Before Start or In Start
- [x] `SetInterval` - Configure and Start the Timer

### Version 2.2.1

- [x] Add TimingSettings.SetMinimumTimerResolution [Uses timeBeginPeriod]
- [x] Add TimingSettings.ClearMinimumTimerResolution [Uses timeEndPeriod]
- [x] Started: Raised Event when PrecisionTimer is Started
- [x] Stopped: Raised Event when PrecisionTimer is Stopped
- [x] IsRunning: True if PrecisionTimer is Running
- [x] Add TimerNotConfigured Exception
- [x] Update XML
