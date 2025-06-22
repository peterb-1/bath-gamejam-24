namespace Gameplay.Ghosts
{
    public class GhostContext
    {
        public GhostRun GhostRun { get; set; }
        public float DisplayTime { get; set; }

        public GhostContext(GhostRun run, float time)
        {
            GhostRun = run;
            DisplayTime = time;
        }
    }
}