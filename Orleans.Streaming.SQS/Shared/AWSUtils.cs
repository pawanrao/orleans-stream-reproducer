using Amazon;

namespace Orleans.Streaming.SQS.Shared
{
    /// <summary>
    /// Some basic utilities methods for AWS SDK
    /// </summary>
    internal static class AWSUtils
    {
        internal static RegionEndpoint GetRegionEndpoint(string zone = "")
        {
            //
            // Keep the order from RegionEndpoint so it is easier to maintain.
            // us-west-2 is the default
            //

            return RegionEndpoint.GetBySystemName(zone) ?? RegionEndpoint.USWest2;
        }
    }
}