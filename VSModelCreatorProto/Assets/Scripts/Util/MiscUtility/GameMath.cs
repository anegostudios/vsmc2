using System.Runtime.CompilerServices;

namespace VSMC
{
    /// <summary>
    /// Taken from base game code. Functions will be added here as and when needed.
    /// </summary>
    public static class GameMath
    {
        #region Modulo
        /// <summary>
        /// C#'s %-Operation is actually not modulo but remainder, so this is the actual modulo function that ensures positive numbers as return value
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int Mod(int k, int n)
        {
            return ((k %= n) < 0) ? k + n : k;
        }

        public static uint Mod(uint k, uint n)
        {
            return ((k %= n) < 0) ? k + n : k;
        }

        /// <summary>
        /// C#'s %-Operation is actually not modulo but remainder, so this is the actual modulo function that ensures positive numbers as return value
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static float Mod(float k, float n)
        {
            return ((k %= n) < 0) ? k + n : k;
        }

        /// <summary>
        /// C#'s %-Operation is actually not modulo but remainder, so this is the actual modulo function that ensures positive numbers as return value
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Mod(double k, double n)
        {
            return ((k %= n) < 0) ? k + n : k;
        }

        #endregion

        /// <summary>
        /// Basic Lerp
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float v0, float v1, float t)
        {
            return v0 + (v1 - v0) * t;
        }

        /// <summary>
        /// Basic Lerp
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double v0, double v1, double t)
        {
            return v0 + (v1 - v0) * t;
        }


    }
}