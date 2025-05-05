namespace VSMC
{
    [System.Serializable]
    public class AttachmentPoint
    {

        /// <summary>
        /// The json code of this attachment point.
        /// </summary>
        public string Code;

        /// <summary>
        /// The X position of the attachment point.
        /// </summary>
        public double PosX;

        /// <summary>
        /// The Y position of the attachment point.
        /// </summary>
        public double PosY;

        /// <summary>
        /// The Z position of the attachment point.
        /// </summary>
        public double PosZ;

        /// <summary>
        /// The forward vertical rotation of the attachment point.
        /// </summary>
        public double RotationX;

        /// <summary>
        /// The forward horizontal rotation of the attachment point
        /// </summary>
        public double RotationY;

        /// <summary>
        /// the left/right tilt of the attachment point
        /// </summary>
        public double RotationZ;
    }
}