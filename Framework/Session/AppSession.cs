namespace Framework.Session
{
    /// <summary>
    /// Application server side session state. Get it with property UtilServer.AppInternal.AppSession
    /// </summary>
    internal sealed class AppSession
    {
        /// <summary>
        /// Gets or sets RequestCount. Managed and incremented by client only.
        /// </summary>
        public int RequestCount;

        /// <summary>
        /// Gets or sets ResponseCount. Managed and incremented by server only.
        /// </summary>
        public int ResponseCount;
    }

    public enum GridRowEnum
    {
        None = 0,

        /// <summary>
        /// Filter row where user enters search text.
        /// </summary>
        Filter = 1,

        /// <summary>
        /// Data row loaded from database.
        /// </summary>
        Index = 2,

        /// <summary>
        /// Data row not yet inserted into database.
        /// </summary>
        New = 3,

        /// <summary>
        /// Data row at the end of the grid showing total.
        /// </summary>
        Total = 4
    }
}
