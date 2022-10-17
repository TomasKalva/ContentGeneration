namespace Assets.Util
{
    class CountdownTimer
    {
        float timeUntilFinished;

        public CountdownTimer(float timeUntilFinished)
        {
            this.timeUntilFinished = timeUntilFinished;
        }

        public bool Finished(float deltaT)
        {
            timeUntilFinished -= deltaT;
            return timeUntilFinished <= 0f;
        }
    }

    class EventsCountdown
    {
        int eventsRemaning;

        public EventsCountdown(int eventsRemaning)
        {
            this.eventsRemaning = eventsRemaning;
        }

        public bool Finished(bool eventHappened)
        {
            eventsRemaning -= eventHappened ? 1 : 0;
            return eventsRemaning <= 0;
        }
    }
}
