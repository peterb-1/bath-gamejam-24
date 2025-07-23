namespace UI.Trails
{
    public interface ITrailDisplayStrategy
    {
        public void Update();
        public void EmitTrail();
        public void StopEmitting();
    }
}