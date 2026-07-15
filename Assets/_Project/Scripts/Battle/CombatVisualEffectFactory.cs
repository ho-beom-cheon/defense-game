using UnityEngine;

namespace RuneGate
{
    public static class CombatVisualEffectFactory
    {
        public static void SpawnSkillEffect(SkillData skillData, Vector3 casterPosition, Vector3 targetPosition, bool hasTarget)
        {
            string key = BuildSkillKey(skillData);
            Vector3 safeTargetPosition = hasTarget ? targetPosition : casterPosition + Vector3.right * 0.85f;

            if (key.Contains("shield"))
            {
                SpawnSpriteOrPulse("SkillFx_ShieldBash", RuntimePixelAssetLoader.EffectShieldBash, safeTargetPosition, Quaternion.identity, new Color(0.24f, 0.62f, 1f, 0.92f), new Vector2(0.86f, 0.86f), 0.28f, 16);
                return;
            }

            if (key.Contains("rapid") || key.Contains("arrow"))
            {
                SpawnSpriteLineOrLine("SkillFx_RapidShot", RuntimePixelAssetLoader.EffectRapidShot, casterPosition + Vector3.up * 0.18f, safeTargetPosition + Vector3.up * 0.18f, new Color(0.35f, 1f, 0.36f, 0.92f), 0.4f, 0.22f, 17);
                return;
            }

            if (key.Contains("meteor") || key.Contains("area_damage") || key.Contains("fire"))
            {
                SpawnSpriteOrPulse("SkillFx_Meteor", RuntimePixelAssetLoader.EffectMeteorImpact, safeTargetPosition + Vector3.up * 0.1f, Quaternion.identity, new Color(1f, 0.38f, 0.16f, 0.94f), new Vector2(1.08f, 1.08f), 0.34f, 18);
                SpawnLine("SkillFx_MeteorDrop", safeTargetPosition + Vector3.up * 1.1f, safeTargetPosition, new Color(1f, 0.62f, 0.18f, 0.72f), 0.1f, 0.28f, 17);
                return;
            }

            if (key.Contains("heal") || key.Contains("holy") || key.Contains("crystal_heal"))
            {
                SpawnSpriteOrPulse("SkillFx_HolyHeal", RuntimePixelAssetLoader.EffectHolyHeal, casterPosition + Vector3.up * 0.28f, Quaternion.identity, new Color(1f, 0.84f, 0.32f, 0.9f), new Vector2(1.05f, 1.05f), 0.4f, 17);
                return;
            }

            if (key.Contains("turret") || key.Contains("engineer"))
            {
                SpawnSpriteOrPulse("SkillFx_TurretShot", RuntimePixelAssetLoader.EffectTurretShot, casterPosition + new Vector3(0.48f, -0.28f, 0f), Quaternion.identity, new Color(0.72f, 0.52f, 0.32f, 0.9f), new Vector2(0.62f, 0.62f), 0.45f, 17);
                return;
            }

            if (key.Contains("shadow") || key.Contains("assassin"))
            {
                Vector3 from = safeTargetPosition + new Vector3(-0.45f, 0.3f, 0f);
                Vector3 to = safeTargetPosition + new Vector3(0.45f, -0.25f, 0f);
                SpawnSpriteLineOrLine("SkillFx_ShadowStrike", RuntimePixelAssetLoader.EffectShadowSlash, from, to, new Color(0.72f, 0.28f, 1f, 0.92f), 0.72f, 0.24f, 18);
                return;
            }

            SpawnPulse("SkillFx_Generic", safeTargetPosition, new Color(0.8f, 0.9f, 1f, 0.78f), new Vector2(0.62f, 0.62f), 0.28f, 16);
        }

        public static void SpawnHitSpark(Vector3 position, float targetHeight)
        {
            float size = Mathf.Clamp(targetHeight * 0.34f, 0.32f, 0.74f);
            SpawnSpriteOrPulse("HitSpark_Runtime", RuntimePixelAssetLoader.EffectHitSpark, position, Quaternion.identity, new Color(1f, 0.95f, 0.45f, 0.92f), new Vector2(size, size), 0.22f, 16);
        }

        public static void SpawnDeathPuff(Vector3 position, float targetHeight)
        {
            float size = Mathf.Clamp(targetHeight * 0.55f, 0.58f, 1.24f);
            SpawnSpriteOrPulse("DeathPuff_Runtime", RuntimePixelAssetLoader.EffectDeathPuff, position, Quaternion.identity, new Color(0.78f, 0.78f, 0.78f, 0.9f), new Vector2(size, size), 0.38f, 15);
        }

        public static void SpawnRapidShotImpact(Vector3 from, Vector3 to)
        {
            SpawnSpriteLineOrLine("SkillFx_RapidShotHit", RuntimePixelAssetLoader.EffectRapidShot, from + Vector3.up * 0.18f, to + Vector3.up * 0.18f, new Color(0.35f, 1f, 0.36f, 0.92f), 0.34f, 0.16f, 17);
        }

        public static void SpawnTurretShot(Vector3 from, Vector3 to)
        {
            SpawnSpriteLineOrLine("SkillFx_TurretProjectile", RuntimePixelAssetLoader.EffectTurretShot, from, to, new Color(1f, 0.72f, 0.25f, 0.94f), 0.24f, 0.18f, 17);
        }

        private static string BuildSkillKey(SkillData skillData)
        {
            if (skillData == null)
            {
                return string.Empty;
            }

            return $"{skillData.SkillId} {skillData.DisplayName} {skillData.EffectKey}".ToLowerInvariant();
        }

        private static void SpawnPulse(string objectName, Vector3 position, Color color, Vector2 size, float lifetime, int sortingOrder)
        {
            GameObject effectObject = new GameObject(objectName);
            effectObject.transform.position = position;
            effectObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = effectObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, size, sortingOrder);
            AutoDestroyEffect autoDestroyEffect = effectObject.AddComponent<AutoDestroyEffect>();
            autoDestroyEffect.Configure(lifetime);
        }

        private static void SpawnSpriteOrPulse(string objectName, string spritePath, Vector3 position, Quaternion rotation, Color color, Vector2 size, float lifetime, int sortingOrder)
        {
            Sprite sprite = RuntimePixelAssetLoader.LoadSprite(spritePath);
            if (sprite == null)
            {
                SpawnPulse(objectName, position, color, size, lifetime, sortingOrder);
                return;
            }

            SpawnSprite(objectName, sprite, position, rotation, color, size, lifetime, sortingOrder);
        }

        private static void SpawnSpriteLineOrLine(string objectName, string spritePath, Vector3 from, Vector3 to, Color color, float thickness, float lifetime, int sortingOrder)
        {
            Vector3 delta = to - from;
            float length = Mathf.Max(0.1f, delta.magnitude);
            Vector3 center = (from + to) * 0.5f;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            Sprite sprite = RuntimePixelAssetLoader.LoadSprite(spritePath);
            if (sprite == null)
            {
                SpawnLine(objectName, from, to, color, Mathf.Max(0.02f, thickness * 0.2f), lifetime, sortingOrder);
                return;
            }

            SpawnSprite(objectName, sprite, center, Quaternion.Euler(0f, 0f, angle), color, new Vector2(length, Mathf.Max(0.08f, thickness)), lifetime, sortingOrder);
        }

        private static void SpawnSprite(string objectName, Sprite sprite, Vector3 position, Quaternion rotation, Color color, Vector2 targetSize, float lifetime, int sortingOrder)
        {
            GameObject effectObject = new GameObject(objectName);
            effectObject.transform.SetPositionAndRotation(position, rotation);
            SpriteRenderer spriteRenderer = effectObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            FitSpriteTransform(effectObject.transform, spriteRenderer, targetSize);
            AutoDestroyEffect autoDestroyEffect = effectObject.AddComponent<AutoDestroyEffect>();
            autoDestroyEffect.Configure(lifetime);
        }

        private static void SpawnLine(string objectName, Vector3 from, Vector3 to, Color color, float thickness, float lifetime, int sortingOrder)
        {
            Vector3 delta = to - from;
            float length = Mathf.Max(0.1f, delta.magnitude);
            Vector3 center = (from + to) * 0.5f;

            GameObject effectObject = new GameObject(objectName);
            effectObject.transform.position = center;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            effectObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            effectObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholderSprite = effectObject.AddComponent<PlaceholderSprite>();
            placeholderSprite.Configure(color, new Vector2(length, Mathf.Max(0.02f, thickness)), sortingOrder);
            AutoDestroyEffect autoDestroyEffect = effectObject.AddComponent<AutoDestroyEffect>();
            autoDestroyEffect.Configure(lifetime);
        }

        private static void FitSpriteTransform(Transform targetTransform, SpriteRenderer spriteRenderer, Vector2 targetSize)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = spriteRenderer.sprite.bounds;
            float width = Mathf.Max(0.01f, bounds.size.x);
            float height = Mathf.Max(0.01f, bounds.size.y);
            targetTransform.localScale = new Vector3(
                Mathf.Max(0.01f, targetSize.x) / width,
                Mathf.Max(0.01f, targetSize.y) / height,
                1f);
        }
    }
}
