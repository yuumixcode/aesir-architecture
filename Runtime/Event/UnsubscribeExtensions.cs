using UnityEngine;

namespace Runestone.AesirArchitecture
{
    public static class UnsubscribeExtensions
    {
        public static void UnsubscribeWhenGameObjectOnDestroyed(this AutoUnsubscribeHandle unsubscribe,
            GameObject gameObject)
        {
            var unsubscribeInvoker = GetOrAddComponent<UnsubscribeOnDestroyInvoker>(gameObject);
            unsubscribeInvoker.AddUnsubscribeHandle(unsubscribe);
        }

        public static void UnsubscribeWhenGameObjectOnDisable(this AutoUnsubscribeHandle unsubscribe,
            GameObject gameObject)
        {
            var unsubscribeInvoker = GetOrAddComponent<UnsubscribeOnDisableInvoker>(gameObject);
            unsubscribeInvoker.AddUnsubscribeHandle(unsubscribe);
        }

        public static void UnsubscribeWhenOnSceneUnloaded(this AutoUnsubscribeHandle unsubscribe)
        {
            UnsubscribeOnSceneUnloadedInvoker.Instance.AddUnsubscribeHandle(unsubscribe);
        }

        static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var unsubscribeInvoker = gameObject.GetComponent<T>();
            if (unsubscribeInvoker == null)
            {
                unsubscribeInvoker = gameObject.AddComponent<T>();
            }

            return unsubscribeInvoker;
        }
    }
}
