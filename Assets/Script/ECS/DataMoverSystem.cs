using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class DataMoverSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeed) => {
            translation.Value += moveSpeed.speed;

            if (translation.Value.x > 5f || translation.Value.y > 5f || translation.Value.z > 5f)
                moveSpeed.speed = -math.abs(moveSpeed.speed);

            if (translation.Value.x < -5f || translation.Value.y < -5f || translation.Value.z < -5f)
                moveSpeed.speed = +math.abs(moveSpeed.speed);
        }).Schedule();
    }
}
