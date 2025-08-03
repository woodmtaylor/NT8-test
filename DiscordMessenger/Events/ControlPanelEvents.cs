using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Events
{
    public class ControlPanelEvents
    {
        private readonly EventManager _eventManager;
        public event Action<Status> OnUpdateStatus;
        public event Action<EventLog> OnUpdateEventLog;
        public event Action<bool> OnAutoButtonClicked;
        public event Func<ProcessType, Task> OnTakeScreenshot;
        public event Func<ProcessType, string, Task> OnScreenshotProcessed;
        public event Action OnAutoScreenshotProcessedWaiting;

        public ControlPanelEvents(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void UpdateEventLog(EventLog eventLog)
        {
            _eventManager.InvokeEvent(OnUpdateEventLog, eventLog);
        }

        public void UpdateStatus(Status status)
        {
            _eventManager.InvokeEvent(OnUpdateStatus, status);
        }

        public void AutoButtonClicked(bool isEnabled)
        {
            _eventManager.InvokeEvent(OnAutoButtonClicked, isEnabled);
        }

        public Task TakeScreenshot(ProcessType processType)
        {
            if (OnTakeScreenshot != null)
            {
                return OnTakeScreenshot.Invoke(processType);
            }
            return Task.FromResult(0);
        }

        public Task ScreenshotProcessed(ProcessType processType, string screenshotName)
        {
            if (OnScreenshotProcessed != null)
            {
                return OnScreenshotProcessed.Invoke(processType, screenshotName);
            }
            return Task.FromResult(0);
        }

        public void AutoScreenshotProcessedWaiting()
        {
            _eventManager.InvokeEvent(OnAutoScreenshotProcessedWaiting);
        }
    }
}