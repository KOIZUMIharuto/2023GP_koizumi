using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class taiko_status : MonoBehaviour
{
	public string Name;
	public string Type;
	public float Size;
	public string Stand;
	public GameObject textPrefab; // インスペクタから TextMeshPro プレハブをアサイン
    private TextMeshProUGUI nameText;
	public GameObject textObject;


	public class Taiko_scores
	{
		public int Replace { get; set; }
		public int Move { get; set; }

		public Taiko_scores()
		{
			Replace = 0;
			Move = 0;
		}
	}
	public Taiko_scores Scores = new Taiko_scores();

	public List<Vector3> taiko_position = new List<Vector3>();
	List<string> taiko_stand = new List<string>();

	public int replace_headcount = 0;
	public int move_headcount = 0;

	Vector3 initial_position;
	string[] stand_array;

	int[] replace_array;

	public TMP_Dropdown dropdownPrefab;
	private TMP_Dropdown dropdownInstance;
	bool can_drag;
	private ray_cast ray_cast;
	int pre_now_number = -1;
	int now_number;
    // Start is called before the first frame update
    void Start()
    {
		// set color black
		GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		initial_position = this.transform.position;


        Canvas canvas = FindObjectOfType<Canvas>();

		textObject = Instantiate(textPrefab, transform.position, Quaternion.identity);
        textObject.transform.SetParent(canvas.transform);

        // TextMeshPro オブジェクトに名前を設定
        nameText = textObject.GetComponent<TextMeshProUGUI>();

		switch (Type)
		{
			case "oke":
				stand_array = new string[] { "eitetsudai", "katsugi" };
				replace_array = new int[] { 0, 1 };
				break;
			case "miya":
				stand_array = new string[] { "shikakudai", "miyakedai", "yataidai" };
				replace_array = new int[] { 0, 3 };
				break;
			case "shime":
				stand_array = new string[] { "zadai", "tachidai" };
				replace_array = new int[] { 0, 1 };
				break;
			case "oodaiko":
				stand_array = new string[] { "hiradai", "yagura" };
				replace_array = new int[] { 0, 3 };
				break;
		}

		Stand = stand_array[0];

		if (nameText != null)
		{
			nameText.text = Name + "\n" + Stand;
		}

		dropdownInstance = Instantiate(dropdownPrefab, transform);
        dropdownInstance.transform.SetParent(canvas.transform);
        // TMP_Dropdownの項目を設定
        dropdownInstance.ClearOptions();
        dropdownInstance.AddOptions(new List<string>(stand_array));

        // TMP_Dropdownの選択イベントにメソッドを登録
        dropdownInstance.onValueChanged.AddListener(OnDropdownValueChanged);
		dropdownInstance.gameObject.SetActive(false);

		this.transform.localScale = new Vector3(Size * 3 / 4, Size* 3 / 4, 1);

		ray_cast = Camera.main.GetComponent<ray_cast>();

    }

    // Update is called once per frame
    void Update()
    {
        if (nameText != null)
        {
            nameText.text = Name + "\n" + Stand;
        }
		can_drag = GameObject.Find("main_system").GetComponent<main_script>().can_drag;
		now_number = GameObject.Find("main_system").GetComponent<main_script>().now_number;
		if (pre_now_number != now_number)
		{
			if (now_number >= taiko_position.Count)
			{
				taiko_position.Add(initial_position);
				taiko_stand.Add(stand_array[0]);

			}
			if (pre_now_number >= 0)
			{
				taiko_position[pre_now_number] = this.transform.position;
				taiko_stand[pre_now_number] = Stand;
			}
			this.transform.position = taiko_position[now_number];
			Stand = taiko_stand[now_number];
			pre_now_number = now_number;
		}
		else
		{
			taiko_position[now_number] = this.transform.position;
			taiko_stand[now_number] = Stand;
		}

		if (textObject != null)
        {
            Vector3 objectPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position);
            textObject.transform.position = objectPosition;
        }
		if (dropdownInstance != null)
		{
			Vector3 objectPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position);
			dropdownInstance.transform.position = objectPosition - new Vector3(0, Size * 20f, 0);
			dropdownInstance.transform.localScale = new Vector3(0.75f, 0.75f, 1);

			if (Input.GetMouseButtonDown(1) && can_drag)
			{
				GameObject lastHitObject = ray_cast.GetLastHitObject();
				if (lastHitObject == gameObject)
				{
					dropdownInstance.gameObject.SetActive(!dropdownInstance.gameObject.activeSelf);
				}
			}
		}
		if (can_drag)
		{
			for (int i = 1; i < taiko_stand.Count; i++)
			{
				if(taiko_stand[i - 1] != taiko_stand[i])
				{
					replace_headcount = replace_array[1];
				}
				else
				{
					replace_headcount = replace_array[0];
				}

				if(Vector3.Distance(taiko_position[i - 1], taiko_position[i]) > 1.0f)
				{
					move_headcount = 1;
				}
				else
				{
					move_headcount = 0;
				}
			}
		}
    }
	void OnMouseDrag()
    {
		if (can_drag)
		{
			Vector3 thisPosition = Input.mousePosition;
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(thisPosition);
			worldPosition.z = 0f;
			this.transform.position = worldPosition;
		}
    }
	void OnDropdownValueChanged(int index)
    {
        Stand = stand_array[index];

		dropdownInstance.gameObject.SetActive(false);
    }
}
