using XLua;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace behaviac
{
[Hotfix]
    public class DecoratorTime : DecoratorNode
    {
        public DecoratorTime()
        {
		}
        ~DecoratorTime()
        {
            this.m_time_var = null;
        }

        protected override void load(int version, string agentType, List<property_t> properties)
        {
			base.load(version, agentType, properties);

			foreach (property_t p in properties)
			{
				if (p.name == "Time")
				{
					string typeName = null;
					string propertyName = null;
					this.m_time_var = Condition.LoadRight(p.value, propertyName, ref typeName);
				}
			}
		}

        protected virtual int GetTime(Agent pAgent)
        {
            if (this.m_time_var != null)
            {
                Debug.Check(this.m_time_var != null);
                int time = (int)this.m_time_var.GetValue(pAgent);

                return time;
            }

            return 0;
        }

        protected override BehaviorTask createTask()
        {
            DecoratorTimeTask pTask = new DecoratorTimeTask();

            return pTask;
        }

        Property m_time_var;

[Hotfix]
        class DecoratorTimeTask : DecoratorTask
        {
            public DecoratorTimeTask()
            {
            }

            ~DecoratorTimeTask()
            {
            }

            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debug.Check(target is DecoratorTimeTask);
                DecoratorTimeTask ttask = (DecoratorTimeTask)target;

                ttask.m_start = this.m_start;
                ttask.m_time = this.m_time;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID startId = new CSerializationID("start");
                node.setAttr(startId, this.m_start);

                CSerializationID timeId = new CSerializationID("time");
                node.setAttr(timeId, this.m_time);
            }
            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            protected override bool onenter(Agent pAgent)
            {
                base.onenter(pAgent);

                this.m_start = 0;
                this.m_time = this.GetTime(pAgent);

                if (this.m_time <= 0)
                {
                    return false;
                }

                return true;
            }

            protected override EBTStatus decorate(EBTStatus status)
            {
				this.m_start += (int)(Time.deltaTime * 1000.0f);
                if (this.m_start >= this.m_time)
                {
                    return EBTStatus.BT_SUCCESS;
                }

                return EBTStatus.BT_RUNNING;
            }

            int GetTime(Agent pAgent)
            {
                Debug.Check(this.GetNode() is DecoratorTime);
                DecoratorTime pNode = (DecoratorTime)(this.GetNode());

                return pNode != null ? pNode.GetTime(pAgent) : 0;
            }

            int m_start;
            int m_time;
        }
    }
}

