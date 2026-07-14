using UnityEngine;

namespace GTA3Unity.Peds
{
    public static class PedManager
    {
        public static Ped SpawnPed(int pedModelIndex, Vector3? position = null, float angle = 0.0f)
        {
            GameObject gameObject = new GameObject();
            var newPed = gameObject.AddComponent<Ped>();
            newPed.SetModel(pedModelIndex);

            if(position != null)
            {
                newPed.transform.position = (Vector3)position;
            }
            newPed.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));

            return newPed;
        }
    }
}
