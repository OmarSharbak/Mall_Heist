using JButler;

public enum LoginOptions
{
    None = 0,
    Guest = 1,
}

public class LootLockerPlayerAuth : PersistentSingleton<LootLockerPlayerAuth>
{
    public LoginOptions SelectedLoginOption = LoginOptions.None;
}