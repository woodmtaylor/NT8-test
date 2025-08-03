using System;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Events
{
    public class EventManager
    {
        public event Action<string> OnPrintMessage;

        public void InvokeEvent(Action eventHandler)
        {
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke();
                }
            }
            catch (Exception ex)
            {
                PrintMessage("Error invoking event: " + ex.Message);
            }
        }

        public void InvokeEvent<T>(Action<T> eventHandler, T arg)
        {
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(arg);
                }
            }
            catch (Exception ex)
            {
                PrintMessage("Error invoking event: " + ex.Message);
            }
        }

        public void InvokeEvent<T1, T2>(Action<T1, T2> eventHandler, T1 arg1, T2 arg2)
        {
            try
            {
                if (eventHandler != null)
                {
                    eventHandler.Invoke(arg1, arg2);
                }
            }
            catch (Exception ex)
            {
                PrintMessage("Error invoking event: " + ex.Message);
            }
        }

        public void PrintMessage(string eventMessage)
        {
            InvokeEvent(OnPrintMessage, eventMessage);
        }
    }
}