using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Play_Script play_script;
    public GameObject Shop_window;
    public Image[] bg_img_skin_btn = new Image[2];

    int shop_status;

    public void Open_Shop()
    {
        shop_status = 1;
    }
    public void Close_Shop()
    {
        shop_status = -1;
    }

    private void Update()
    {
        if (shop_status != 0)
            Shop_window_move();
    }

    // показываем/прячем окно магазина
    void Shop_window_move()
    {
        // shop_status: 1 - показываем окно, -1 - скрываем.
        if (shop_status == 1)
        {
            Shop_window.transform.Translate(0, float.Parse(0.55.ToString()), 0);
            if (Shop_window.transform.position.y >= 0)
                shop_status = 0;
        }
        else if (shop_status == -1)
        {
            Shop_window.transform.Translate(0, -float.Parse(0.55.ToString()), 0);
            if (Shop_window.transform.position.y <= -11)
                shop_status = 0;
        }
    }

    // смена скина
    public void Choose_skin(int num)
    {
        for (int i = 0; i < bg_img_skin_btn.Length; i++)
            bg_img_skin_btn[i].enabled = false;
        bg_img_skin_btn[num].enabled = true;

        PlayerPrefs.SetInt("skin_num", num);
        play_script.Set_Skin(num);
    }
}
