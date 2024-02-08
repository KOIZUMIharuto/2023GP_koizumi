using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;


public class GameObjectStringPair
{
    public GameObject Taiko { get; set; }
    public string Parameter { get; set; }
}
public class taikoName_task
{
    public string Taiko { get; set; }
    public string Task { get; set; }
}

public class playerName_task
{
    public string Player { get; set; }
    public List<taikoName_task> Task { get; set; }
}
public class taiko_task_player_set
{
    public GameObject Taiko { get; set; }
    public string Task { get; set; }
	public GameObject Player { get; set; }
}
public class main_script : MonoBehaviour
{
	public GameObject player_prefab;
	public GameObject taiko_prefab;

	public List<GameObject> players = new List<GameObject>();
	public List<GameObject> taikos = new List<GameObject>();

	public TextMeshProUGUI now_number_text;
	public TextMeshProUGUI percentage_text;

	long total_taiko_pattern_count = 0;
	long total_player_pattern_count = 0;
	int excess_player_count = 0;
	float total_pattern_count = 0;
	long calculated_pattern_count = 0;
	float percentage = 0;

	public int now_number = 0;
	public bool can_drag = true;
	public bool is_calculating = false;
	[System.Serializable]
	public class player_data
	{
		public string Name;
		public int Greade;
		public int Base_operation;
		public int Replace_shime;
		public int Move_shime;
		public int Replace_miya;
		public int Replace_oodaiko;
		public int Move_oodaiko;
	}

	[System.Serializable]
	public class taiko_data
	{
		public string Name;
		public string Type;
		public float Size;
		public int Replace;
		public int Move;
	}

	int player_count;
	int taiko_count;

	int score_deadline = 0;
	
	List<float> score_tmp_list = new List<float>();

	public float distance_score_weight = 0.1f;
	public float parameter_score_weight = 2f;
	public float total_score;
	List<List<taiko_task_player_set>> taiko_task_player_list = new List<List<taiko_task_player_set>>();

	List<playerName_task> allocated_task_list = new List<playerName_task>();

    void Start()
    {
		now_number_text.text = "first number";
		// hide percentage_text
		percentage_text.GetComponent<TextMeshProUGUI>().enabled = false;

		string taiko_json_path = "Assets/Resources/taiko_test_data.json";
		string player_json_path = "Assets/Resources/player_test_data.json";
		float screen_width = 18f;
		if (File.Exists(player_json_path) && File.Exists(taiko_json_path))
		{
			string player_json = File.ReadAllText(player_json_path);
			player_data[] playerDataArray = JsonHelper.FromJson<player_data>(player_json);
			float interval = screen_width / (playerDataArray.Length + 1);
			for (int i = 0; i < playerDataArray.Length; i++)
			{
				float player_x = interval * (i + 1) - screen_width / 2;
				float player_y = 4f;
				players.Add(Instantiate(player_prefab, new Vector3(player_x, player_y, 0), Quaternion.identity));
				players[i].GetComponent<player_status>().Name = playerDataArray[i].Name;
				players[i].GetComponent<player_status>().Greade = playerDataArray[i].Greade;
				players[i].GetComponent<player_status>().Scores.Base_operation = playerDataArray[i].Base_operation;
				players[i].GetComponent<player_status>().Scores.Replace_shime = playerDataArray[i].Replace_shime;
				players[i].GetComponent<player_status>().Scores.Move_shime = playerDataArray[i].Move_shime;
				players[i].GetComponent<player_status>().Scores.Replace_miya = playerDataArray[i].Replace_miya;
				players[i].GetComponent<player_status>().Scores.Replace_oodaiko = playerDataArray[i].Replace_oodaiko;
				players[i].GetComponent<player_status>().Scores.Move_oodaiko = playerDataArray[i].Move_oodaiko;
				allocated_task_list.Add(new playerName_task
				{
					Player = players[i].GetComponent<player_status>().Name,
					Task = new List<taikoName_task>()
				});
				player_count++;
			}
		}
		if (File.Exists(taiko_json_path) && File.Exists(player_json_path))
		{
			string taiko_json = File.ReadAllText(taiko_json_path);
			taiko_data[] taikoDataArray = JsonHelper.FromJson<taiko_data>(taiko_json);
			float interval = screen_width / (taikoDataArray.Length + 1);
			for (int i = 0; i < taikoDataArray.Length; i++)
			{
				float taiko_x = interval * (i + 1) - screen_width / 2;
				float taiko_y = -4f;
				taikos.Add(Instantiate(taiko_prefab, new Vector3(taiko_x, taiko_y, 0), Quaternion.identity));
				taikos[i].GetComponent<taiko_status>().Name = taikoDataArray[i].Name;
				taikos[i].GetComponent<taiko_status>().Type = taikoDataArray[i].Type;
				taikos[i].GetComponent<taiko_status>().Size = taikoDataArray[i].Size;
				taikos[i].GetComponent<taiko_status>().Scores.Replace = taikoDataArray[i].Replace;
				taikos[i].GetComponent<taiko_status>().Scores.Move = taikoDataArray[i].Move;
				taiko_count++;
			}
		}
    }

    // Update is called once per frame
    void Update()
    {

		change_number();
		if (Input.GetKeyDown(KeyCode.Return) && !can_drag && !is_calculating)
		{
			// show percentage_text
			percentage_text.GetComponent<TextMeshProUGUI>().enabled = true;
			total_score = Mathf.Infinity;
			excess_player_count = 0;
			int total_task_count = get_task_count();
			int total_turn_count = total_task_count / player_count;
			int left_task_count = total_task_count % player_count;
			if (left_task_count != 0)
			{
				total_turn_count++;
			} else
			{
				left_task_count = player_count;
			}

			// 総パターン数計算
			total_taiko_pattern_count = (long)(Mathf.Pow(Mathf.Pow(total_turn_count, 2), taiko_count));
			total_player_pattern_count = (long)(Mathf.Pow(factorial(player_count), total_turn_count - 1) * permutation(player_count, left_task_count));
			total_pattern_count = total_taiko_pattern_count * total_player_pattern_count;
			calculated_pattern_count = 0;
			percentage = calculated_pattern_count / total_pattern_count * 100;
			percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";

			Debug.Log("total_task_count" + total_task_count + " player_count" + player_count + " total_turn_count" + total_turn_count + " total_pattern_count" + total_pattern_count.ToString("F0"));
			List<List<GameObjectStringPair>> task_list = new List<List<GameObjectStringPair>>();
			taiko_task_player_list = new List<List<taiko_task_player_set>>();
			score_tmp_list = new List<float>();

			for (int i = 0; i < total_turn_count; i++)
			{
				task_list.Add(new List<GameObjectStringPair>());
				taiko_task_player_list.Add(new List<taiko_task_player_set>());
			}
			StartCoroutine(RunRecursiveTask(total_turn_count, task_list));
			
		}

    }
	IEnumerator RunRecursiveTask(int total_turn_count, List<List<GameObjectStringPair>> task_list)
	{
		is_calculating = true;
		int taiko_index = 0;
		Debug.Log("start");
		yield return StartCoroutine(recursive_task_sort(taiko_index, total_turn_count, task_list));
		Debug.Log("end");
		Debug.Log("score" + total_score);
		allocated_task_list = new List<playerName_task>();
		foreach (GameObject player in players)
		{
			allocated_task_list.Add(new playerName_task
			{
				Player = player.GetComponent<player_status>().Name,
				Task = new List<taikoName_task>()
			});
		}
		foreach (List<taiko_task_player_set> taiko_task_player_list_of_turn in taiko_task_player_list)
		{
			foreach (taiko_task_player_set taiko_task_player in taiko_task_player_list_of_turn)
			{
				allocated_task_list
					.Find(x => x.Player == taiko_task_player.Player.GetComponent<player_status>().Name)
					.Task.Add(new taikoName_task
						{
							Taiko = taiko_task_player.Taiko.GetComponent<taiko_status>().Name,
							Task = taiko_task_player.Task
						});
			}
		}
		foreach (playerName_task player_task in allocated_task_list)
		{
			string debugText = "\n" + player_task.Player + "\n";
			foreach (taikoName_task taiko_task in player_task.Task)
			{
				debugText += "\t" + taiko_task.Taiko + " " + taiko_task.Task + "\n";
			}
			Debug.Log(debugText);
		}
		// output all data of allocated_task_list to json
		string json = CreateJsonString(allocated_task_list);
		Debug.Log(json);
		// output json to new file
		// make unique file name  with timestamp
		string path = "Assets/Resources/allocated_task_list" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
		// write json to file
		File.WriteAllText(path, json);
		Debug.Log("total_pattern_count" + total_pattern_count.ToString("F0") + " calculated_pattern_count" + calculated_pattern_count.ToString("F0"));		
		is_calculating = false;
	}

	IEnumerator recursive_task_sort(int taiko_index, int total_turn_count, List<List<GameObjectStringPair>> task_list)
	{
		if (taiko_index == taiko_count)
		{
			if(!check_over_player_count(task_list))
			{
				calculated_pattern_count += total_player_pattern_count;
				percentage = calculated_pattern_count / total_pattern_count * 100;
				percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
				yield break;
			}
			List<List<GameObjectStringPair>> formatted_task_list = format_task_list(task_list);
			List<List<GameObject>> player_list = new List<List<GameObject>>();
			for(int i = 0; i < total_turn_count; i++)
			{
				player_list.Add(new List<GameObject>());
			}
			yield return StartCoroutine(recursive_advance_turn(0, formatted_task_list, player_list));
			yield break;
		}
		for(int replace_turn = 0; replace_turn < total_turn_count; replace_turn++)
		{
			bool is_replace_headcount_zero = false;
			if(taikos[taiko_index].GetComponent<taiko_status>().replace_headcount != 0)
			{
				task_list[replace_turn].Add(new GameObjectStringPair
				{
					Taiko = taikos[taiko_index],
					Parameter = "Replace"
				});
			}
			else
			{
				is_replace_headcount_zero = true;
			}

			for(int move_turn = 0; move_turn < total_turn_count; move_turn++)
			{
				bool is_move_headcount_zero = false;
				if(taikos[taiko_index].GetComponent<taiko_status>().move_headcount != 0)
				{
					if(replace_turn == move_turn && !is_replace_headcount_zero)
					{
						calculated_pattern_count += (long)(Mathf.Pow(Mathf.Pow(total_turn_count, 2), taiko_count - taiko_index - 1) * total_player_pattern_count);
						percentage = calculated_pattern_count / total_pattern_count * 100;
						percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
						continue;
					}
					task_list[move_turn].Add(new GameObjectStringPair
					{
						Taiko = taikos[taiko_index],
						Parameter = "Move"
					});
				}
				else
				{
					is_move_headcount_zero = true;
				}
				yield return StartCoroutine(recursive_task_sort(taiko_index + 1, total_turn_count, task_list));
				if(!is_move_headcount_zero)
				{
					task_list[move_turn].RemoveAt(task_list[move_turn].Count - 1);
				}
				else
				{
					calculated_pattern_count += (long)((total_turn_count - 1) * Mathf.Pow(Mathf.Pow(total_turn_count, 2), taiko_count - taiko_index - 1) * total_player_pattern_count);
					percentage = calculated_pattern_count / total_pattern_count * 100;
					percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
					break;
				}
			}
			if(!is_replace_headcount_zero)
			{
				task_list[replace_turn].RemoveAt(task_list[replace_turn].Count - 1);
			}
			else
			{
				calculated_pattern_count += (long)((total_turn_count - 1) * total_turn_count * Mathf.Pow(Mathf.Pow(total_turn_count, 2), taiko_count - taiko_index - 1) * total_player_pattern_count);
				percentage = calculated_pattern_count / total_pattern_count * 100;
				percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
				break;
			}
		}
	}
	List<List<taiko_task_player_set>> copy_taiko_task_player_list(List<List<taiko_task_player_set>> originalList)
    {
        List<List<taiko_task_player_set>> newList = new List<List<taiko_task_player_set>>();
		foreach (List<taiko_task_player_set> originalSubList in originalList)
		{
			List<taiko_task_player_set> newSubList = new List<taiko_task_player_set>();
			foreach (taiko_task_player_set originalItem in originalSubList)
			{
				newSubList.Add(new taiko_task_player_set
				{
					Taiko = originalItem.Taiko,
					Task = originalItem.Task,
					Player = originalItem.Player
				});
			}
			newList.Add(newSubList);
		}
		return newList;
    }

	IEnumerator calculate_score(List<List<GameObjectStringPair>> formatted_task_list, List<List<GameObject>> player_list)
	{
		float score_tmp = 0f;
		List<List<taiko_task_player_set>> taiko_task_player_list_tmp = new List<List<taiko_task_player_set>>();
		Dictionary<GameObject, List<Vector3>> taiko_position_list = new Dictionary<GameObject, List<Vector3>>();
		Dictionary<GameObject, List<Vector3>> player_position_list = new Dictionary<GameObject, List<Vector3>>();

		for(int i = 0; i < taiko_count; i++)
		{
			List<Vector3> taiko_position = new List<Vector3>();
			taiko_position_list.Add(taikos[i], taiko_position);
			taiko_position_list[taikos[i]].Add(taikos[i].GetComponent<taiko_status>().taiko_position[0]);
		}
		for(int i = 0; i < player_count; i++)
		{
			List<Vector3> player_position = new List<Vector3>();
			player_position_list.Add(players[i], player_position);
			player_position_list[players[i]].Add(players[i].GetComponent<player_status>().player_position[0]);
		}

		for(int i = 0; i < player_list.Count; i++)
		{
			taiko_task_player_list_tmp.Add(new List<taiko_task_player_set>());
			for(int j = 0; j < player_list[i].Count; j++)
			{
				int player_task_score = 0;
				int taiko_task_score = 0;
				GameObject player_object = player_list[i][j];
				GameObject taiko_object = formatted_task_list[i][j].Taiko;
				taiko_task_player_list_tmp[i].Add(new taiko_task_player_set
				{
					Taiko = taiko_object,
					Task = formatted_task_list[i][j].Parameter,
					Player = player_object
				});

				float distance_from_player_to_taiko = Vector3.Distance(
					player_position_list[player_object].Last(),
					taiko_position_list[taiko_object].Last()
				);

				switch(formatted_task_list[i][j].Parameter)
				{
					case "Replace":
						taiko_task_score = taiko_object.GetComponent<taiko_status>().Scores.Replace;
						taiko_position_list[taiko_object].Add(taiko_position_list[taiko_object].Last());
						player_position_list[player_object].Add(taiko_position_list[taiko_object].Last());
						switch(taiko_object.GetComponent<taiko_status>().Type)
						{
							case "shime":
								player_task_score = player_object.GetComponent<player_status>().Scores.Replace_shime;
								break;
							case "miya":
								player_task_score = player_object.GetComponent<player_status>().Scores.Replace_miya;
								break;
							case "oodaiko":
								player_task_score = player_object.GetComponent<player_status>().Scores.Replace_oodaiko;
								break;
							default:
								player_task_score = player_object.GetComponent<player_status>().Scores.Base_operation;
								break;
						}
						break;
					case "Move":
						taiko_task_score = taiko_object.GetComponent<taiko_status>().Scores.Move;
						taiko_position_list[taiko_object].Add(taiko_object.GetComponent<taiko_status>().taiko_position[1]);
						player_position_list[player_object].Add(taiko_object.GetComponent<taiko_status>().taiko_position[1]);
						switch (taiko_object.GetComponent<taiko_status>().Type)
						{
							case "shime":
								player_task_score = player_object.GetComponent<player_status>().Scores.Move_shime;
								break;
							case "oodaiko":
								player_task_score = player_object.GetComponent<player_status>().Scores.Move_oodaiko;
								break;
							default:
								player_task_score = player_object.GetComponent<player_status>().Scores.Base_operation;
								break;
						}
						break;
				}
				if (player_task_score < score_deadline){
					
					// percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
					yield break;
				}
				// 1曲目と2曲のやつ入れなきゃ
				// 太鼓とプレイヤーの移動を加味していない
				score_tmp += distance_from_player_to_taiko * distance_score_weight + Mathf.Max(0, taiko_task_score - player_task_score) * parameter_score_weight;
			}
		}
		foreach(GameObject player in players)
		{
			float player_distance_to_next_position = Vector3.Distance(
				player_position_list[player].Last(),
				player.GetComponent<player_status>().player_position[1]
			);
			score_tmp += player_distance_to_next_position * distance_score_weight;
		}
		if(score_tmp < total_score)
		{
			score_tmp_list.Add(score_tmp);
			total_score = score_tmp;
			taiko_task_player_list = copy_taiko_task_player_list(taiko_task_player_list_tmp);
		}
		yield break;
	}

	IEnumerator recursive_advance_turn(int turn_index, List<List<GameObjectStringPair>> formatted_task_list, List<List<GameObject>> player_list)
	{
		if (turn_index == formatted_task_list.Count)
		{
			// スコア計算
			// bool is_updated = calculate_score(formatted_task_list, player_list, true);
			yield return StartCoroutine(calculate_score(formatted_task_list, player_list));
			calculated_pattern_count += 1;
			percentage = calculated_pattern_count / total_pattern_count * 100;
			percentage_text.text = "score: " + total_score + "\n" + percentage.ToString("F3") + "% (" + calculated_pattern_count.ToString("F0") + ")";
			yield break;
		} else {
			List<GameObject> players_copy = new List<GameObject>(players);
			// bool is_under_score = calculate_score(formatted_task_list, player_list, false);
			// if (is_under_score)
			// {
			// 	calculated_pattern_count += Mathf.Pow(factorial(player_count), formatted_task_list.Count - turn_index) * permutation(player_count, formatted_task_list.Last().Count);
			// 	yield break;
			// } else {
			// 	yield return StartCoroutine(recursive_task_allocate(turn_index, formatted_task_list, player_list, players_copy));
			// }
			yield return StartCoroutine(recursive_task_allocate(turn_index, formatted_task_list, player_list, players_copy));
		}

	}

	IEnumerator recursive_task_allocate(int turn_index, List<List<GameObjectStringPair>> formatted_task_list, List<List<GameObject>> player_list, List<GameObject> players_copy)
	{
		if(player_count - players_copy.Count == formatted_task_list[turn_index].Count)
		{
			yield return StartCoroutine(recursive_advance_turn(turn_index + 1, formatted_task_list, player_list));
			yield break;
		}
		for(int i = 0; i < players_copy.Count; i++)
		{
			player_list[turn_index].Add(players_copy[i]);
			List<GameObject> players_copy_copy = new List<GameObject>(players_copy);
			players_copy_copy.RemoveAt(i);
			yield return StartCoroutine(recursive_task_allocate(turn_index, formatted_task_list, player_list, players_copy_copy));
			player_list[turn_index].RemoveAt(player_list[turn_index].Count - 1);
		}
	}

	List<List<GameObjectStringPair>> format_task_list(List<List<GameObjectStringPair>> task_list)
	{
		List<List<GameObjectStringPair>> formatted_task_list = new List<List<GameObjectStringPair>>();
		foreach(List<GameObjectStringPair> task_of_turn in task_list)
		{
			List<GameObjectStringPair> formatted_task_of_turn = new List<GameObjectStringPair>();
			foreach(GameObjectStringPair task in task_of_turn)
			{
				if(task.Parameter == "Replace")
				{
					for(int i = 0; i < task.Taiko.GetComponent<taiko_status>().replace_headcount; i++)
					{
						formatted_task_of_turn.Add(new GameObjectStringPair
						{
							Taiko = task.Taiko,
							Parameter = "Replace"
						});
					}
				}
				else if(task.Parameter == "Move")
				{
					for(int i = 0; i < task.Taiko.GetComponent<taiko_status>().move_headcount; i++)
					{
						formatted_task_of_turn.Add(new GameObjectStringPair
						{
							Taiko = task.Taiko,
							Parameter = "Move"
						});
					}
				}
			}
			formatted_task_list.Add(formatted_task_of_turn);
		}
		return formatted_task_list;
	}

	bool check_over_player_count(List<List<GameObjectStringPair>> task_list)
	{
		for(int i = 0; i < task_list.Count; i++)
		{
			int total_headcount = 0;
			foreach (GameObjectStringPair task in task_list[i])
			{
				if (task.Parameter == "Replace")
				{
					total_headcount += task.Taiko.GetComponent<taiko_status>().replace_headcount;
				}
				else if (task.Parameter == "Move")
				{
					total_headcount += task.Taiko.GetComponent<taiko_status>().move_headcount;
				}
			}
			if ((i != task_list.Count - 1 && total_headcount != player_count) || (i == task_list.Count - 1 && total_headcount > player_count))
			{
				return false;
			}
		}
		return true;
	}

	void change_number()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			now_number = 0;
			now_number_text.text = "first number";
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			now_number = 1;
			now_number_text.text = "second number";
		}
		if (Input.GetKeyDown(KeyCode.Space) && !is_calculating)
		{
			can_drag = !can_drag;
		}
	}

	int get_task_count(){
		int task_count = 0;
		foreach (GameObject taiko in taikos)
		{
			task_count += taiko.GetComponent<taiko_status>().replace_headcount;
			if(taiko.GetComponent<taiko_status>().move_headcount > 1)
			{
				excess_player_count += taiko.GetComponent<taiko_status>().move_headcount - 1;
			}
			task_count += taiko.GetComponent<taiko_status>().move_headcount;
			if (taiko.GetComponent<taiko_status>().move_headcount > 1)
			{
				excess_player_count += taiko.GetComponent<taiko_status>().move_headcount - 1;
			}
		}
		return task_count;
	}
	int factorial(int n)
    {
        if (n < 0)
        {
            return -1;
        }

        if (n == 0 || n == 1)
        {
            return 1;
        }

        int result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }

        return result;
    }
	int permutation(int n, int m)
    {
        if (n < 0 || m < 0 || m > n)
        {
            return -1;
        }

        return factorial(n) / factorial(n - m);
    }
	
	string CreateJsonString(List<playerName_task> data)
    {
        // 文字列ビルダーを利用してJSON文字列を構築
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[\n"); // ルートの開始

        foreach (var playerTask in data)
        {
            sb.Append("\t{\n"); // オブジェクトの開始

            // Player プロパティ
            sb.Append("\t\t\"Player\":\"" + playerTask.Player + "\",\n");
            
            // Task プロパティ
            sb.Append("\t\t\"Task\":[\n");

            foreach (var task in playerTask.Task)
            {
                sb.Append("\t\t\t{\n"); // オブジェクトの開始

                // Taiko プロパティ
                sb.Append("\t\t\t\t\"Taiko\":\"" + task.Taiko + "\",\n");
                
                // Task プロパティ
                sb.Append("\t\t\t\t\"Task\":\"" + task.Task + "\"\n");

                sb.Append("\t\t\t},"); // オブジェクトの終了
            }

            if (playerTask.Task.Count > 0)
            {
                // 最後のカンマを除去
                sb.Length -= 1;
            }

            sb.Append("\n\t\t]\n"); // Task プロパティの終了

            sb.Append("\t},"); // オブジェクトの終了
        }

        if (data.Count > 0)
        {
            // 最後のカンマを除去
            sb.Length -= 1;
        }

        sb.Append("\n]"); // ルートの終了

        return sb.ToString();
    }
}

