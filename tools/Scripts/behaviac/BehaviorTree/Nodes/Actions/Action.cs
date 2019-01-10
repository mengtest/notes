using XLua;
using System;
using System.Collections;
using System.Collections.Generic;

namespace behaviac
{
    // ============================================================================
[Hotfix]
    public class Action : BehaviorNode
    {
        public Action()
        {
            m_resultOption = EBTStatus.BT_INVALID;
			m_resultPreconditionFail = EBTStatus.BT_FAILURE;
        }
        ~Action()
        {
            m_method = null;
            m_resultFunctor = null;
        }

        static string ParseInstanceName(string fullName, ref string instanceName)
        {
            //Self.AgentActionTest::Action2(0)
            int pClassBegin = fullName.IndexOf('.');
            Debug.Check(pClassBegin != -1);

            instanceName = fullName.Substring(0, pClassBegin);

            string propertyName = fullName.Substring(pClassBegin + 1);
            return propertyName;
        }

        static int ParseMethodNames(string fullName, ref string agentIntanceName, ref string agentClassName, ref string methodName)
        {
            //Self.test_ns::AgentActionTest::Action2(0)
            int pClassBegin = fullName.IndexOf('.');
            Debug.Check(pClassBegin != -1);

            agentIntanceName = fullName.Substring(0, pClassBegin);

            int pBeginAgentClass = pClassBegin + 1;

            int pBeginP = fullName.IndexOf('(', pBeginAgentClass);
            Debug.Check(pBeginP != -1);

            //test_ns::AgentActionTest::Action2(0)
            int pBeginMethod = fullName.LastIndexOf(':', pBeginP);
            Debug.Check(pBeginMethod != -1);
            //skip '::'
            Debug.Check(fullName[pBeginMethod] == ':' && fullName[pBeginMethod - 1] == ':');
            pBeginMethod += 1;

            int pos1 = pBeginP - pBeginMethod;

            methodName = fullName.Substring(pBeginMethod, pos1);

            int pos = pBeginMethod - 2 - pBeginAgentClass;

            agentClassName = fullName.Substring(pBeginAgentClass, pos).Replace("::", ".");

            return pBeginP;
        }

        //suppose params are seprated by ','
        static List<string> ParseForParams(string tsrc)
        {
            int tsrcLen = tsrc.Length;
            int startIndex = 0;
            int index = 0;
            int quoteDepth = 0;

            List<string> params_ = new List<string>();

            for (; index < tsrcLen; ++index)
            {
                if (tsrc[index] == '"')
                {
                    quoteDepth++;

                    //if (quoteDepth == 1)
                    //{
                    //	startIndex = index;
                    //}

                    if ((quoteDepth & 0x1) == 0)
                    {
                        //closing quote
                        quoteDepth -= 2;
                        Debug.Check(quoteDepth >= 0);
                    }
                }
                else if (quoteDepth == 0 && tsrc[index] == ',')
                {
                    //skip ',' inside quotes, like "count, count"
                    int lengthTemp = index - startIndex;
                    string strTemp = tsrc.Substring(startIndex, lengthTemp);
                    params_.Add(strTemp);
                    startIndex = index + 1;
                }
            }//end for

            // the last param
            int lengthTemp0 = index - startIndex;
            if (lengthTemp0 > 0)
            {
                string strTemp = tsrc.Substring(startIndex, lengthTemp0);
                params_.Add(strTemp);

                //params_.Add(strTemp);
            }

            return params_;
        }

        public static CMethodBase LoadMethod(string value_)
        {
            //Self.test_ns::AgentActionTest::Action2(0)
            string agentIntanceName = null;
            string agentClassName = null;
            string methodName = null;
            int pBeginP = ParseMethodNames(value_, ref agentIntanceName, ref agentClassName, ref methodName);

            //propertyName = FormatString("%s::%s", agentClassName, methodName);
            CStringID agentClassId = new CStringID(agentClassName);
            CStringID methodId = new CStringID(methodName);

            CMethodBase method = Agent.CreateMethod(agentClassId, methodId);

            if (method == null)
            {
				behaviac.Debug.LogWarning(string.Format("No Method {0}::{1} registered\n", agentClassName, methodName));
				Debug.Check(false, string.Format("No Method {0}::{1} registered\n", agentClassName, methodName));
            }
            else
            {
                if (Agent.IsNameRegistered(agentIntanceName))
                {
                    method.SetInstanceNameString(agentIntanceName, ParentType.PT_INSTANCE);
                }
                else
                {
                    //Debug.Check(agentIntanceName == "Self");
                }

                Debug.Check(method != null, string.Format("No Method {0}::{1} registered", agentClassName, methodName));
                string params_ = value_.Substring(pBeginP);

                Debug.Check(params_[0] == '(');

                List<string> paramsTokens = null;

                {
                    int len = params_.Length;

                    Debug.Check(params_[len - 1] == ')');

                    string text = params_.Substring(1, len - 2);
                    //StringUtils::SplitIntoArray(text, ",", tokens);
                    paramsTokens = ParseForParams(text);
                }

                if (paramsTokens != null)
                {
                    method.Load(null, paramsTokens);
                }
            }

            return method;
        }

        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            foreach (property_t p in properties)
            {
                if (p.value == null) continue;
                if (p.name == "Method")
                {
                    if (!string.IsNullOrEmpty(p.value))
                    {
                        this.m_method = Action.LoadMethod(p.value);
                    }//if (p.value[0] != '\0')
                }
                else if (p.name == "ResultOption")
                {
                    if (p.value == "BT_INVALID")
                    {
                        m_resultOption = EBTStatus.BT_INVALID;
                    }
                    else if (p.value == "BT_FAILURE")
                    {
                        m_resultOption = EBTStatus.BT_FAILURE;
                    }
                    else if (p.value == "BT_RUNNING")
                    {
                        m_resultOption = EBTStatus.BT_RUNNING;
                    }
                    else
                    {
                        m_resultOption = EBTStatus.BT_SUCCESS;
                    }
                }
                else if (p.name == "ResultFunctor")
                {
                    if (p.value[0] != '\0')
                    {
                        this.m_resultFunctor = Action.LoadMethod(p.value);
                    }
                }
				else if (p.name == "PreconditionFailResult")
				{
					if (p.value == "BT_FAILURE")
					{
						m_resultPreconditionFail = EBTStatus.BT_FAILURE;
					}
					else if (p.value == "BT_BT_SUCCESS")
					{
						m_resultPreconditionFail = EBTStatus.BT_SUCCESS;
					}
					else
					{
						Debug.Check(false);
					}
				}
                else
                {
                    //Debug.Check(0, "unrecognised property %s", p.name);
                }
            }
        }

        public override bool IsValid(Agent pAgent, BehaviorTask pTask)
        {
            if (!(pTask.GetNode() is Action))
            {
                return false;
            }

            return base.IsValid(pAgent, pTask);
        }

        protected override BehaviorTask createTask()
        {
            ActionTask pTask = new ActionTask();

            return pTask;
        }

        protected CMethodBase m_method;
        EBTStatus m_resultOption;
        CMethodBase m_resultFunctor;
		EBTStatus m_resultPreconditionFail;

[Hotfix]
        class ActionTask : LeafTask
        {
            public ActionTask()
            {

            }
            ~ActionTask()
            {
            }

            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);
            }
            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            protected override bool onenter(Agent pAgent)
            {
                return true;
            }
            protected override void onexit(Agent pAgent, EBTStatus s)
            {
			}

            static int ms_lastNodeId = -2;

            static void SetNodeId(int nodeId)
            {
                ms_lastNodeId = nodeId;
            }

            static void ClearNodeId()
            {
                ms_lastNodeId = -2;
            }

            public static int GetNodeId()
            {
                return ms_lastNodeId;
            }

            protected override EBTStatus update(Agent pAgent, EBTStatus childStatus)
            {
                Debug.Check(this.GetNode() is Action, "node is not an Action");
                Action pActionNode = (Action)(this.GetNode());

				if (!this.CheckPredicates(pAgent))
				{
					return pActionNode.m_resultPreconditionFail;
				}

				EBTStatus result = EBTStatus.BT_SUCCESS;

                if (pActionNode.m_method != null)
                {
                    ParentType pt = pActionNode.m_method.GetParentType();
                    Agent pParent = pAgent;
                    if (pt == ParentType.PT_INSTANCE)
                    {
                        pParent = Agent.GetInstance(pActionNode.m_method.GetInstanceNameString(), pParent.GetContextId());
						Debug.Check(pParent != null || Utils.IsStaticClass(pActionNode.m_method.GetInstanceNameString()));
                    }

                    int nodeId = this.GetId();
                    SetNodeId(nodeId);

                    object returnValue = pActionNode.m_method.run(pParent, pAgent);

                    if (pActionNode.m_resultOption != EBTStatus.BT_INVALID)
                    {
                        result = pActionNode.m_resultOption;
                    }
                    else if (pActionNode.m_resultFunctor != null)
                    {
                        ParentType pt1 = pActionNode.m_resultFunctor.GetParentType();
                        Agent pParentCheckResult = pAgent;
                        if (pt1 == ParentType.PT_INSTANCE)
                        {
                            pParentCheckResult = Agent.GetInstance(pActionNode.m_resultFunctor.GetInstanceNameString(), pParent.GetContextId());
							Debug.Check(pParentCheckResult != null || Utils.IsStaticClass(pActionNode.m_resultFunctor.GetInstanceNameString()));
                        }

                        result = (EBTStatus)pActionNode.m_resultFunctor.run(pParentCheckResult, pAgent, returnValue);
                    }
                    else
                    {
                        Debug.Check(returnValue is EBTStatus, "method's return type is not EBTStatus");
                        result = (EBTStatus)returnValue;
                    }

                    ClearNodeId();
                }
                else
                {
                    result = pActionNode.update_impl(pAgent, childStatus);
                }

                return result;
            }
        }
    }
}

