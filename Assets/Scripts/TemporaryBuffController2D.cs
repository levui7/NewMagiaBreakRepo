using System.Collections.Generic;
using UnityEngine;

public class TemporaryBuffController2D : MonoBehaviour
{
    public enum BuffType
    {
        Power,
        Speed,
        PhysicalDamage,
        FireDamage,
        WaterDamage
    }

    private class ActiveBuff
    {
        public BuffType type;
        public float multiplier;
        public float expiresAt;
    }

    [Header("Default multipliers")]
    public float defaultPowerMultiplier = 1.25f;
    public float defaultSpeedMultiplier = 1.2f;
    public float defaultElementMultiplier = 1.25f;

    private readonly List<ActiveBuff> buffs = new List<ActiveBuff>();

    public void AddPowerBuff(float duration, float multiplier = -1f)
    {
        AddBuff(BuffType.Power, duration, multiplier <= 0f ? defaultPowerMultiplier : multiplier);
    }

    public void AddSpeedBuff(float duration, float multiplier = -1f)
    {
        AddBuff(BuffType.Speed, duration, multiplier <= 0f ? defaultSpeedMultiplier : multiplier);
    }

    public void AddPhysicalDamageBuff(float duration, float multiplier = -1f)
    {
        AddBuff(BuffType.PhysicalDamage, duration, multiplier <= 0f ? defaultElementMultiplier : multiplier);
    }

    public void AddFireDamageBuff(float duration, float multiplier = -1f)
    {
        AddBuff(BuffType.FireDamage, duration, multiplier <= 0f ? defaultElementMultiplier : multiplier);
    }

    public void AddWaterDamageBuff(float duration, float multiplier = -1f)
    {
        AddBuff(BuffType.WaterDamage, duration, multiplier <= 0f ? defaultElementMultiplier : multiplier);
    }

    public void AddBuff(BuffType type, float duration, float multiplier)
    {
        duration = Mathf.Max(0.1f, duration);
        multiplier = Mathf.Max(0.01f, multiplier);

        buffs.Add(new ActiveBuff
        {
            type = type,
            multiplier = multiplier,
            expiresAt = Time.time + duration
        });
    }

    private void Update()
    {
        if (buffs.Count == 0)
            return;

        float now = Time.time;
        buffs.RemoveAll(buff => buff == null || buff.expiresAt <= now);
    }

    public float GetMoveSpeedMultiplier()
    {
        float result = 1f;

        foreach (ActiveBuff buff in buffs)
        {
            if (buff != null && buff.type == BuffType.Speed)
                result *= buff.multiplier;
        }

        return result;
    }

    public float GetDamageMultiplier(WeaponManager.Element element)
    {
        float result = 1f;

        foreach (ActiveBuff buff in buffs)
        {
            if (buff != null && buff.type == BuffType.Power)
                result *= buff.multiplier;
        }

        switch (element)
        {
            case WeaponManager.Element.Fire:
                result *= GetSpecificMultiplier(BuffType.FireDamage);
                break;

            case WeaponManager.Element.Water:
                result *= GetSpecificMultiplier(BuffType.WaterDamage);
                break;

            case WeaponManager.Element.Physical:
            default:
                result *= GetSpecificMultiplier(BuffType.PhysicalDamage);
                break;
        }

        return result;
    }

    private float GetSpecificMultiplier(BuffType type)
    {
        float result = 1f;

        foreach (ActiveBuff buff in buffs)
        {
            if (buff != null && buff.type == type)
                result *= buff.multiplier;
        }

        return result;
    }

    public void ClearTemporaryBuffs()
    {
        buffs.Clear();
    }
}