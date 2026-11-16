using UnityEngine;
using System.Collections.Generic;

public static class Inventory 
{
    public static Dictionary<string, int> items;
    
    public static string[] equipSlots = new string[3];
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {    
        items = new Dictionary<string, int>();
        equipSlots = new string[3];
    }

    /// <summary> 아이템 개수 추가해주는 함수 (추후 상점과 연결 필요) </summary>
    /// <param name="key"> 아이템 키 받아오는 매개 변수 </param>
    public static void ItemPlus(string key)
    {
        if(items.ContainsKey(key))
            items[key] ++;
        else // !items.ContainsKey(key) 
            items.Add(key, 1);
    }

    /// <summary> 아이템 개수 계산해주는 함수 </summary>
    /// <param name="key"> 아이템 키 받아오는 매개 변수 </param>
    public static int ItemCount(string key)
    {
        return items.ContainsKey(key) ? items[key] : 0;  
    }

    public static void EquipItem()
    {

        // if ()   // 작성 중
    }


}
