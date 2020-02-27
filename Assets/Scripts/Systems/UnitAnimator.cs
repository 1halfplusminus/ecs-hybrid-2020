
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class UnitAnimator : ComponentSystem
{
    protected override void OnUpdate()
    {

        Entities.WithAll<UnitData>().ForEach((Entity entity, ref UnitData unitData) =>
        {
            var animator = EntityManager.GetComponentObject<Animator>(entity);
            if (unitData.animation == UnitData.AnimationType.Walking)
            {
                Debug.Log(UnitData.AnimationType.Walking.ToString());
                animator.SetBool("Walking", true);
            }
            if (unitData.animation == UnitData.AnimationType.Idle)
            {
                animator.SetBool("Walking", false);
            }
        });
    }
}