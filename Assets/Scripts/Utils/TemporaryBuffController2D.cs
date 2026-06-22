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

    private class BuffEntry
    {
        public BuffType type;
        public float multiplier;
        public float expiresAt;
    }

    [Header("Defaults")]
    public float defaultPowerMultiplier = 1.25f;
    public float defaultSpeedMultiplier = 1.2f;
    public float defaultElementMultiplier = 1.25f;

    private readonly List<BuffEntry> activeBuffs = new List<BuffEntry>();

    private void Update()
    {
        float now = Time.time;
        activeBuffs.RemoveAll(b => b == null || b.expiresAt <= now);
    }

    public void AddBuff(BuffType type, float duration, float multiplier)
    {
        activeBuffs.Add(new BuffEntry
        {
            type = type,
            multiplier = Mathf.Max(0.01f, multiplier),
            expiresAt = Time.time + Mathf.Max(0.1f, duration)
        });
    }

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

    public float GetMoveSpeedMultiplier()
    {
        float result = 1f;

        foreach (BuffEntry buff in activeBuffs)
        {
            if (buff != null && buff.type == BuffType.Speed)
                result *= buff.multiplier;
        }

        return result;
    }

    public float GetDamageMultiplier(WeaponManager.Element element)
    {
        float result = 1f;

        foreach (BuffEntry buff in activeBuffs)
        {
            if (buff == null)
                continue;

            if (buff.type == BuffType.Power)
                result *= buff.multiplier;

            if (element == WeaponManager.Element.Physical && buff.type == BuffType.PhysicalDamage)
                result *= buff.multiplier;

            if (element == WeaponManager.Element.Fire && buff.type == BuffType.FireDamage)
                result *= buff.multiplier;

            if (element == WeaponManager.Element.Water && buff.type == BuffType.WaterDamage)
                result *= buff.multiplier;
        }

        return result;
    }

    public void ClearTemporaryBuffs()
    {
        activeBuffs.Clear();
    }
}