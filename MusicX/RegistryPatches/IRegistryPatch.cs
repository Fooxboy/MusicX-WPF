namespace MusicX.RegistryPatches
{
    public interface IRegistryPatch
    {
        public bool CanRun();
        public int Number { get; }
        public bool RequiredAdmin { get; }

        public void Run();
    }
}
