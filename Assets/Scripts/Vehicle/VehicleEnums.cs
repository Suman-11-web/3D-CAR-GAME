namespace CarGame.Vehicle
{
    /// <summary>
    /// Automatic transmission selector modes.
    /// </summary>
    public enum GearMode
    {
        P, // Park: Transmission locked, wheels braked
        R, // Reverse: Reverse gear active
        N, // Neutral: Disengaged drivetrain, freewheeling
        D  // Drive: Forward gears 1-6 automatic shifting
    }

    /// <summary>
    /// Drivetrain layout configuration.
    /// </summary>
    public enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }

    /// <summary>
    /// Camera perspective modes.
    /// </summary>
    public enum CameraViewMode
    {
        Chase,    // Standard 3rd person chase view (reference screenshot)
        FarChase, // Higher and further back 3rd person view
        Cockpit,  // Driver perspective inside the cabin
        Hood      // Bonnet/Hood camera view
    }
}
