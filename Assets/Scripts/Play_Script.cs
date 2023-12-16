using UnityEngine;
using UnityEngine.UI;

public class Play_Script : MonoBehaviour
{
    public ADS_Manager ads_Manager_script;
    public Shop shop_script;
    // стартовое время и добавочное время
    static double start_time = 7, bonuse_add_time = 4;

    // кол. хп орка (изначально - 10, с каждым респауном увеличивается), счёт игрока, урон игрока
    // game over: 0 - играем, 1 - проиграли, 2 - выход в меню
    int HP, score = 0, damage = 1, respawm, settings_status, menu_status, game_over, skin_num, score_to_ads, resume_count;
    double HP_max, time;
    bool sound_status, timer_status, start_game;
    public Image sound_on, sound_off, music_on, music_off;

    // табо счёта игрока
    public Text score_text, best_score_text, timer_text, add_time_text;

    public GameObject damage_button, hp_bar, sound, music, settingss, btn_pause, btn_play, menu, timer, add_time, main_menu, in_game_menu, game_over_menu, btn_resume;
    public Animator[] anim = new Animator[2];
    public AudioSource audio_music, audio_click_button_settings, audio_click_button, audio_z_step_1, audio_z_step_2, audio_z_take_damage, audio_z_falling, audio_z_dying;

    private void Start()
    {
        // преднастройки
        HP = 10; HP_max = HP;
        respawm = 0; settings_status = -2;
        sound.GetComponent<Button>().enabled = false;
        music.GetComponent<Button>().enabled = false;
        sound.SetActive(false); music.SetActive(false);
        sound_status = true; time = start_time;
        Update_timer(); timer_status = false;
        menu_status = 0; start_game = true;
        hp_bar.SetActive(false); game_over = 0;
        damage_button.SetActive(false);

        if (!PlayerPrefs.HasKey("skin_num"))
        {
            PlayerPrefs.SetInt("skin_num", 0);
            skin_num = 0;
        }
        else
            skin_num = PlayerPrefs.GetInt("skin_num");
        Set_Skin(skin_num);
        shop_script.Choose_skin(skin_num);

        if (!PlayerPrefs.HasKey("ADS"))
        {
            PlayerPrefs.SetInt("ADS", 1);
            score_to_ads = 0;
        }
        else
        {
            if (PlayerPrefs.GetInt("ADS") == 1)
                score_to_ads = 0;
            else
                score_to_ads = -1;
        }

        // debug
        Debug.Log("open the APP");
    }

    private void Update()
    {
        if (timer_status)
        {
            time -= Time.deltaTime;
            if (time > 0)
                Update_timer();
            else
            {
                timer_status = false;
                game_over = 1;
                Game_Over();
                //Save_the_score();
                Debug.Log("YOU LOSE!");
            }
        }
    }

    private void FixedUpdate()
    {
        // орк после респацна входит в центр экрана
        if (respawm > 0 && transform.position.x <= 2)
            Orc_Walk();

        // скрываем/показваем настройки
        if (settings_status == -1 || settings_status == 1)
            show_hide_settings();

        if (menu_status != 0)
            Menu_show_hide();

        if (add_time.activeSelf)
            Time_add_move();

        if (resume_count == 0 && btn_resume.activeSelf)
            btn_resume.SetActive(false);
    }

    public void Set_Skin(int x)
    {
        for (int i = 0; i < anim.Length; i++)
            anim[i].gameObject.SetActive(false);
        anim[x].gameObject.SetActive(true);
        skin_num = x;
    }

    // старт игры 
    public void Start_game()
    {
        btn_resume.SetActive(true); resume_count = 2; menu_status = -1; respawn_orc();
    }

    // получение урона орком
    public void Take_damage()
    {
        HP -= damage;
        Debug.Log("Orc take a damage! (new HP = " + HP + "); score_to_ads - " + score_to_ads);

        if (HP <= 0)
        {
            if (sound_status)
            {
                audio_z_take_damage.Stop();
                audio_z_dying.Play();
            }
            // увеличиваем счёт игрока
            score += 1;
            damage_button.SetActive(false);
            hp_bar.SetActive(false);
            timer_status = false;

            if (score_to_ads == -1) // если реклама отключена
                Orc_dying_temp();
            else
            {
                score_to_ads += 1;
                if (score_to_ads == 3)
                {
                    score_to_ads = 0;
                    ADS_Manager.ShowAdsVideo("Interstitial_Android");
                }
                else
                    Orc_dying_temp();
            }
        }
        else
        {
            if (sound_status && (!audio_z_take_damage.isPlaying || audio_z_take_damage.time > 0.6))
                audio_z_take_damage.Play();
            anim[skin_num].Play("Hurt");

            hp_bar.GetComponent<Image>().fillAmount = float.Parse((HP / HP_max).ToString());
        }
    }

    public void Orc_dying_temp()
    {
        if (sound_status)
        {
            audio_z_take_damage.Stop();
            audio_z_dying.Play();
        }
        anim[skin_num].Play("Dying");
        Time_add(bonuse_add_time);
    }

    // проигрывание звука падения умирающего орка
    public void Falling_orc_soud()
    {
        if (sound_status)
            audio_z_falling.Play();
    }

    // функция респауна орка
    public void respawn_orc()
    {
        // орк будет выходить с боку
        // здесь должна вызываться анимация респауна орка
        // пока не проиграется респаун - урон не получается
        Debug.Log("respawn");
        HP = 10 + score * 2; HP_max = HP;
        hp_bar.GetComponent<Image>().fillAmount = 1;

        transform.Translate(-10, 0, 0);
        respawm = 2;
        audio_z_step_1.Play();
    }

    // функция ходьбы орка
    void Orc_Walk()
    {
        if (sound_status)
        {
            if (audio_z_step_1.time > 0.3)
            {
                audio_z_step_1.volume += 0.025F;
                audio_z_step_1.Stop();
                audio_z_step_2.Play();
            }
            else if (audio_z_step_2.time > 0.3)
            {
                audio_z_step_2.volume += 0.025F;
                audio_z_step_2.Stop();
                audio_z_step_1.Play();
            }
        }

        // включаем анимацию ходьбы
        if (respawm == 2)
        {
            anim[skin_num].Play("Walking");
            respawm = 1;
        }

        // двигаем орка в центр экрана
        transform.Translate(float.Parse(0.07.ToString()), 0, 0);

        // включаем анимацию покоя
        if (transform.position.x >= 2)
        {
            audio_z_step_1.volume = 0.03F;
            audio_z_step_2.volume = 0.03F;
            Debug.Log("wtf");

            anim[skin_num].Play("Idle");
            respawm = 0;

            if (!main_menu.activeSelf)
                hp_bar.SetActive(true);

            // если не пауза - то пускаем таймер
            if (btn_pause.activeSelf)
            {
                damage_button.SetActive(true);
                timer_status = true;
            }
        }
    }

    void Time_add_move()
    {

        if (add_time.transform.position.y <= 6)
            add_time.transform.Translate(0, float.Parse(0.035.ToString()), 0);
        else
            add_time.SetActive(false);
    }

    // добавление времени к таймеру
    void Time_add(double T)
    {
        // если T < 0, то это купленое время оно не добавляется а просто присваивается как при старте
        add_time.SetActive(true);
        add_time.transform.position = new Vector3(float.Parse(0.4.ToString()), float.Parse(3.3.ToString()), 100);
        if (T < 0)
            add_time_text.text = "+ " + T * -1 + " sec.";
        else
            add_time_text.text = "+ " + T + " sec.";

        if (T < 0)
            time = T * -1;
        else
            time += T;

        Debug.Log("time is add");

        Update_timer();
    }

    // обновляем показания таймера
    void Update_timer()
    {
        timer_text.text = ": " + time.ToString("0.0") + " sec.";
    }

    // пауза игры
    public void Game_Pause()
    {
        btn_pause.SetActive(false);
        damage_button.SetActive(false);
        timer_status = false;

        menu_status = 1;
    }

    // возобновление игры
    public void Game_Play()
    {
        btn_play.SetActive(false);
        menu_status = -1;
    }

    // показываем/прячем меню
    void Menu_show_hide()
    {
        // menu_satus: 1 - показываем меню, -1 - скрываем.
        if (menu_status == 1)
        {
            menu.transform.Translate(-float.Parse(0.55.ToString()), 0, 0);

            if (timer.transform.position.y <= 2.2)
                timer.transform.Translate(0, float.Parse(0.25.ToString()), 0);

            if (menu.transform.position.x <= 0.7)
            {
                menu_status = 0;
                if (game_over != 1)
                    btn_play.SetActive(true);
            }
        }
        else if (menu_status == -1)
        {
            menu.transform.Translate(float.Parse(0.55.ToString()), 0, 0);

            if (menu.transform.position.x > 3 && timer.transform.position.y >= 1 && game_over == 0)
                timer.transform.Translate(0, -float.Parse(0.25.ToString()), 0);

            if (menu.transform.position.x >= 6.5 && game_over != 2)
            {
                if (game_over_menu.activeSelf)
                {
                    game_over_menu.SetActive(false);
                    in_game_menu.SetActive(true);
                }

                menu_status = 0;
                btn_pause.SetActive(true);

                if (start_game)
                {
                    start_game = false;
                    main_menu.SetActive(false);
                    game_over_menu.SetActive(false);
                    in_game_menu.SetActive(true);
                    btn_pause.SetActive(true);
                }

                if (respawm == 0)
                {
                    damage_button.SetActive(true);
                    timer_status = true;
                }
            }
            else if (menu.transform.position.x >= 6.5 && game_over == 2)
            {
                main_menu.SetActive(true);
                game_over_menu.SetActive(false);
                in_game_menu.SetActive(false);
                menu_status = 1; game_over = 0;
                start_game = true;
            }
        }
    }

    // проигрыш
    void Game_Over()
    {
        // подтягиваем лучший результат игрока
        if (PlayerPrefs.HasKey("score"))
            best_score_text.text = PlayerPrefs.GetInt("score").ToString();
        else
            best_score_text.text = "---";

        score_text.text = score.ToString();
        in_game_menu.SetActive(false);
        game_over_menu.SetActive(true);
        btn_pause.SetActive(false);
        damage_button.SetActive(false);
        timer_status = false;
        menu_status = 1;
        Save_the_score();
    }

    // рестарт
    public void Restart()
    {
        resume_count = 2; btn_resume.SetActive(true);
        time = start_time;
        game_over = 0; score = 0;
        hp_bar.SetActive(false);
        Update_timer();
        Start_game();
    }

    // продолжить с доп. временем
    public void Resume()
    {
        resume_count -= 1;
        ADS_Manager.ShowAdsVideo("Rewarded_Android");
    }
    public void Resume_after_ads()
    {
        Time_add(bonuse_add_time * -1); game_over = 0;
        damage_button.SetActive(true);
        menu_status = -1;
    }

    // выйти в главное меню
    public void Back_to_main_menu()
    {
        time = start_time; Update_timer();
        hp_bar.SetActive(false);
        HP = 10; HP_max = HP;
        score = 0;
        game_over = 2; menu_status = -1;
    }

    // отключение рекламы
    public void ADS_off_sound()
    {
        if (sound_status)
            audio_click_button.Play();
    }
    public void ADS_off()
    {
        if (PlayerPrefs.GetInt("ADS") == 1)
        {
            Debug.Log("отключение рекламы");
            PlayerPrefs.SetInt("ADS", 0);
            score_to_ads = -1;
            ads_Manager_script.ADS_OFF();
        }
        else
        {
            Debug.Log("реклама уже отключена; включится при следующем запуске!");
            PlayerPrefs.SetInt("ADS", 1);
        }

        //PlayerPrefs.SetInt("ADS", 1);
        //score_to_ads = -1;
        //ads_Manager_script.ADS_OFF();
    }

    public void ADS_buy_failed()
    {
        Debug.Log("походу денег нету!");
    }

    // открыть/закрыть настройки
    public void settings()
    {
        if (sound_status)
            audio_click_button_settings.Play();
        // README
        // -2 - настройки скрыты, -1 - настройки скрываются, 1 - настройки открываются, 2 - настройки открыты
        if (settings_status == -2)
        {
            settings_status = 1;
            Debug.Log("открыть настройки");
        }
        else if (settings_status == 2)
        {
            settings_status = -1;
            Debug.Log("закрыть настройки");
        }
    }

    // открытие-закрытие настроек
    void show_hide_settings()
    {
        // открываем настройки
        if (settings_status == 1)
        {
            //при первом заходе - включаем кнопки
            if (!music.activeSelf && !sound.activeSelf)
            {
                sound.SetActive(true); music.SetActive(true);
            }

            music.transform.Translate(float.Parse(0.1.ToString()), 0, 0);
            if (music.transform.position.x >= settingss.transform.position.x + 1)
                sound.transform.Translate(float.Parse(0.1.ToString()), 0, 0);

            if (music.transform.position.x >= settingss.transform.position.x + 1.93)
            {
                settings_status = 2;
                sound.GetComponent<Button>().enabled = true;
                music.GetComponent<Button>().enabled = true;
            }
        }
        else if (settings_status == -1)
        {
            //при первом заходе - включаем кнопки
            if (sound.GetComponent<Button>().isActiveAndEnabled && music.GetComponent<Button>().isActiveAndEnabled)
            { sound.GetComponent<Button>().enabled = false; music.GetComponent<Button>().enabled = false; }

            if (sound.transform.position.x > settingss.transform.position.x)
            {
                sound.transform.Translate(-float.Parse(0.1.ToString()), 0, 0);
                music.transform.Translate(-float.Parse(0.1.ToString()), 0, 0);
            }
            else if (sound.transform.position.x <= settingss.transform.position.x && music.transform.position.x > settingss.transform.position.x)
            {
                if (sound.activeSelf)
                    sound.SetActive(false);

                music.transform.Translate(-float.Parse(0.1.ToString()), 0, 0);
            }
            else if (music.transform.position.x <= settingss.transform.position.x)
            { music.SetActive(false); settings_status = -2; }
        }
    }

    // вкл.-выкл. звук
    public void settings_sound_click()
    {
        if (sound_status)
        {
            sound_status = false;
            sound.GetComponent<Image>().sprite = sound_off.sprite;
        }
        else
        {
            audio_click_button.Play();
            sound_status = true;
            sound.GetComponent<Image>().sprite = sound_on.sprite;
        }
    }

    // вкл.-выкл. музыку
    public void settings_music_click()
    {
        if (sound_status)
            audio_click_button.Play();
        if (audio_music.volume == 0.15F)
        {
            audio_music.volume = 0;
            music.GetComponent<Image>().sprite = music_off.sprite;
        }
        else
        {
            audio_music.volume = 0.15F;
            music.GetComponent<Image>().sprite = music_on.sprite;
        }
    }

    // вызвать таблицу рекордов
    public void Table_score()
    {
        if (sound_status)
            audio_click_button.Play();
        Debug.Log("вызвать таблицу рекордов");
    }

    // записываем результат если это новый рекорд
    void Save_the_score()
    {
        if (!PlayerPrefs.HasKey("score"))
            PlayerPrefs.SetInt("score", score);
        else if (PlayerPrefs.GetInt("score") < score)
            PlayerPrefs.SetInt("score", score);

        Debug.Log("maybe save score = " + score);
    }

    // функция закрывающая приложение
    public void App_close()
    {
        if (sound_status)
            audio_click_button.Play();

        Application.Quit();
    }
}
