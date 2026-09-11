using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using GameManagerCalls;
using System.Collections.Generic;
using Spells;

public class SpellVolumeManager : MonoBehaviour
{
    [SerializeField]
    private Volume blurVolume;
    private float _lastAppliedTimeScale = 1f;

    [SerializeField]
    private float removeWeightTime = 0.2f;

    private float startRemoveTime;
    private bool _removing = false;

    private void Update()
    {
        float currentTimeScale = Time.timeScale;
        if (Mathf.Approximately(currentTimeScale, _lastAppliedTimeScale))
            return;

        if (SpellManager.Instance.ActiveSpell == null)
        {
            if(blurVolume.weight > 0)
            {
                if (!_removing)
                {
                    startRemoveTime = Time.unscaledTime;
                    _removing = true;
                }

                float endRemoveTime = startRemoveTime + removeWeightTime;

                blurVolume.weight = Mathf.Clamp01(Mathf.InverseLerp(startRemoveTime, endRemoveTime, Time.unscaledTime));
            }
            return;
        }
        _removing = false;

        _lastAppliedTimeScale = currentTimeScale;

        blurVolume.weight = 1f - currentTimeScale;
    }
}
