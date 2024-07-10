public class OdometryMessage
{
    public Header? header { get; set; }
    public string? child_frame_id { get; set; }
    public PoseWithCovariance? pose { get; set; }
    public TwistWithCovariance? twist { get; set; }

    public class Header
    {
        public int seq { get; set; }
        public double stamp { get; set; }
        public string? frame_id { get; set; }
    }

    public class PoseWithCovariance
    {
        public Pose? pose { get; set; }
        public double[]? covariance { get; set; }

        public class Pose
        {
            public Position? position { get; set; }
            public Orientation? orientation { get; set; }

            public class Position
            {
                public double x { get; set; }
                public double y { get; set; }
                public double z { get; set; }
            }

            public class Orientation
            {
                public double x { get; set; }
                public double y { get; set; }
                public double z { get; set; }
                public double w { get; set; }
            }
        }
    }

    public class TwistWithCovariance
    {
        public Twist? twist { get; set; }
        public double[]? covariance { get; set; }

        public class Twist
        {
            public Vector3? linear { get; set; }
            public Vector3? angular { get; set; }

            public class Vector3
            {
                public double x { get; set; }
                public double y { get; set; }
                public double z { get; set; }
            }
        }
    }
}
