using System.IO;
using UnityEngine;

// https://projectcerbera.com/gta/3/tutorials/handling
// https://projectcerbera.com/gta/3-vc/tutorials/detailed-acceleration
// "heavy vehicle is given more power by the game than a light vehicle with the same Engine Acceleration rate. This is why some heavy trucks can reach a higher top speeds than the light sports cars."
namespace GTA3Unity.Vehicles
{
    public class TransmissionData
    {
        public int NumberOfGears; // Doesn't appear to do much outside of audio
        public float MaxVelocity;
        public float EngineAcceleration;
        public EDriveType DriveType;
        public EEngineType EngineType;
    }

    public class HandlingData
    {
        public string VehicleIdentifier; // Relates this data with default.ide and other files
        public float Mass; // Mass of the vehicle in kilograms
        public Vector3 Dimensions; // Width, Length & Height of the vehicle in metres, used for aerodynamic and motion effects
        public Vector3 CentreOfMass; // Distance from the centre of the car in metres
        public int PercentSubmerged; // Percentage of the vehicle height required to be submerged for the car to float
        public float TractionMultiplier; // Cornering grip of the vehicle as a multiplier of the tyre surface friction
        public float TractionLoss; // Accelerating/braking grip of the vehicle as a multiplier of the tyre surface friction
        public float TractionBias; // Ratio of front axle grip to rear axle grip; higher value shifts grip forwards
        public TransmissionData TransmissionData = new();
        public float BrakeDeceleration;
        public float BrakeBias;
        public float ABS; // Unused in GTA3, keeping for compatibility sake though
        public float SteeringLock;
        public float SuspensionForceLevel;
        public float SuspensionDampingLevel;
        public float SeatOffsetDistance;
        public float CollisionDamageMultiplier;
        public int MonetaryValue; // Apparently determines how much the crusher give the player
        public int FrontLights; // What does this even do?
        public int RearLights; // What does this even do too?
    }

    public static class HandlingManager
    {
        public static void LoadHandlingData(string handlingCfg)
        {
            if(!File.Exists(handlingCfg))
            {
                Debug.LogError($"Failed to load \"{handlingCfg}\": File does not exist!");
                return;
            }

            string[] lines = File.ReadAllLines(handlingCfg);
            foreach(var line in lines)
            {
                if(line.StartsWith(';'))
                {
                    // ; is a comment
                    continue;
                }

                // TODO: Implement handling.cfg reading
            }
        }
    }
}