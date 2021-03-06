﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Networking;
using Tobii.Plugins;
using UnityEngine;
using UnityEngine.Networking;

namespace Survey
{
    public class LoadSurveyFromWeb : MonoBehaviour
    {
        public bool Loading;

        public List<QuestionDetails> SurveyList { get; private set; }


        private void Start()
        {
            Loading = true;
        }

        private static List<string> SeparatePipeInString(string pipestuff)
        {
            return Regex.Split(pipestuff, @"\|").ToList();
        }


        private void JsonNodeToListList(JSONNode data)
        {
            Debug.Log(data["data"]);
            Debug.Log(data["data"][0]);
            SurveyList = new List<QuestionDetails>();
            for (var i = 0; i < data["data"].Count; ++i)
            {
                var tempList =
                    ScriptableObject.CreateInstance<QuestionDetails>();
                switch (data["data"][i].Count)
                {
                    case 5:
                    {
                        tempList.OfferedAnswerList =
                            SeparatePipeInString(data["data"][i][4]);
                        goto case 4;
                    }

                    case 4:
                    {
                        tempList.OfferedAnswerId = data["data"][i][3];
                        goto case 3;
                    }
                    case 3:
                    {
                        tempList.QuestionString = data["data"][i][2];
                        goto case 2;
                    }
                    case 2:
                    {
                        tempList.QuestionType = data["data"][i][1];
                        goto case 1;
                    }
                    case 1:
                    {
                        tempList.QuestionId = data["data"][i][0];
                        break;
                    }
                }


                SurveyList.Add(tempList);
                Debug.Log(string.Format(
                    "QuestionId: {0}\nQuestionType{1}\nQuestionString: {2},OfferedId: {3}\n",
                    tempList.QuestionId, tempList.QuestionType,
                    tempList.QuestionString, tempList.OfferedAnswerId));
            }

            Debug.Log("List begins");
            Debug.Log(SurveyList);
            Debug.Log("List ends");
            Loading = false;
        }

        private void String2DArrayToListList(string[][] data)
        {
            Debug.Log(data);
            Debug.Log(data[0]);
            SurveyList = new List<QuestionDetails>();
            for (var i = 0; i < data.Length; ++i)
            {
                var tempList =
                    ScriptableObject.CreateInstance<QuestionDetails>();
                switch (data[i].Length)
                {
                    case 4:
                    {
                        tempList.OfferedAnswerList =
                            SeparatePipeInString(data[i][3]);
                        goto case 3;
                    }
                    case 3:
                    {
                        tempList.QuestionString = data[i][2];
                        goto case 2;
                    }
                    case 2:
                    {
                        tempList.QuestionType = data[i][1];
                        Debug.Log(tempList.QuestionType +
                                  " is in Load survey, i = " + i);
                        goto case 1;
                    }
                    case 1:
                    {
                        tempList.QuestionId = data[i][0];
                        break;
                    }
                }


                SurveyList.Add(tempList);
            }

            Debug.Log("List begins");
            Debug.Log(SurveyList);
            Debug.Log("List ends");
            Loading = false;
        }

        public IEnumerator LoadSurveyEnumerator(int surveyNumber)
        {
            var form = new WWWForm();
            form.AddField("survey_id", surveyNumber);

            using (var www = UnityWebRequest.Post(ServerURL.RETRIEVE_SURVEY,
                form)) {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    var result = new[]
                    {
                        new[]
                        {
                            "0", "Numeric",
                            "This is a dummy Numeric question because internet is not working.",
                            "8-100"
                        },
                        new[]
                        {
                            "1", "Multiple",
                            "This is a dummy Multiple question because internet is not working.",
                            "Yes|@No"
                        },
                        new[]
                        {
                            "2", "Multiple",
                            "This is a dummy Multiple question because internet is not working.",
                            "@Yes|No"
                        },
                        new[]
                        {
                            "3", "Scalar",
                            "This is a dummy Scalar question because internet is not working.",
                            "@one|@two|@three|four|five"
                        },
                        new[]
                        {
                            "4", "PickAll",
                            "This is a dummy PickAll question because internet is not working.",
                            "cool1|coool2|cool3|coool4|cool5|coool6"
                        }
                    };
                    String2DArrayToListList(result);
                }
                /*
                 * case "Numeric":
                        NumericGetAnswer(g, ref temp);
                        break;
                    //special cases bellow
                    case "IfYesRespond":
                        IfYesRespondGetAnswer(g, ref temp);
                        break;
                    case "IfNoRespond":
                        IfYesRespondGetAnswer(g, ref temp);
                        break;
                    case "IfScalarLessThan3Respond":
                        IfScalarLessThan3RespondGetAnswer(g, ref temp);
                        break;
                    case "Scale":
                        ScaleGetAnswer(g, ref temp);
                        break;
                }
                 */
                else
                {
                    var result = JSON.Parse(www.downloadHandler.text);
                    Debug.Log(result + " is result");

                    if (result["failed"].AsBool)
                    {
                        Debug.Log(result["error"]);
                        Application.Quit();
                    }
                    else
                    {
                        var temp = result["data"];
                        JsonNodeToListList(result);
                    }
                }
            }
        }
    }
}