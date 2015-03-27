namespace ModTools
{
    public interface IModEntryPoint
    {
        void OnModLoaded();

        void OnModUnloaded();
    }
}
