using System;
using System.Timers;

namespace RegistratorWorker.Common
{
    public class RegisterTimer
    {
        private Action _action;
        private TimeSpan _interval;
        public RegisterTimer(Action action,TimeSpan interval)
        {
            _action = action;
            _interval = interval;
        }

        public void Start()
        {
            var timer = new Timer(_interval.TotalMilliseconds);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _action();
        }
    }
}
