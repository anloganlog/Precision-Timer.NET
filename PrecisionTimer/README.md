### Version 2.2.4.7
- [x] Remove Extra Layer
- [x] Remove throw from `Callback`
- [x] Rename `AutoReset` to `AutoResetMode`
- [x] Tick event can be null
- [x] Refactor some things
- [x] Add Locks
- [x] Using `Action` without `Tick Event` is now faster
- [x] Volatile
- [x] Add `Configure` overload as alternative to `SetInterval`
- [x] Re-add Getter Methods
- [x] Re-add Setter Methods
- [x] Add `Overloads`
- [x] Add Exception `PeriodicTimerRunningException`
- [x] Change `TimingSettings` to `PrecisionTimerSettings`
- [x] Adds `PrecisionTimerEvent` which is a simplified `EventHandler` only version of `PrecisionTimer`

### Version 2.2.4.6
- [x] Garbage Collection Changes
- [x] More Detailed Example
- [x] Fix some edge cases
- [x] Make it easier to change `Resolution`

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
