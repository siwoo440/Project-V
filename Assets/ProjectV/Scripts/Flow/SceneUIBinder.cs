using UnityEngine; // Unity 기본 기능

public static class SceneUIBinder // 이름 기반 UI 연결
{
    public static T Bind<T>(T current, string objectName) where T : Component
    {
        if (current != null) { return current; } // 인스펙터 연결 우선

        GameObject targetObject = GameObject.Find(objectName); // 이름으로 검색

        if (targetObject == null)
        {
            Debug.LogWarning($"UI 오브젝트를 찾지 못했습니다: {objectName}"); // 오브젝트 누락 경고
            return null;
        }

        T component = targetObject.GetComponent<T>(); // 컴포넌트 검색

        if (component == null)
        {
            Debug.LogWarning(
                $"{objectName}에 {typeof(T).Name} 컴포넌트가 없습니다."
            ); // 컴포넌트 누락 경고
        }

        return component;
    }
}
