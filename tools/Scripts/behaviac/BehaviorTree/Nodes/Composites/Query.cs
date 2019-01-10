using XLua;
using System;
using System.Collections;
using System.Collections.Generic;

namespace behaviac
{
[Hotfix]
    public class Query : BehaviorNode
    {
        public Query()
        {
		}

        ~Query()
        {
		}

        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            if (properties.Count > 0)
            {
                foreach (property_t p in properties)
                {
                    if (p.name == "Domain")
                    {
                        m_domain = p.value;
                    }
                    else if (p.name == "Descriptors")
                    {
                        SetDescriptors(p.value);
                    }
                    else
                    {
                        //Debug.Check(0, "unrecognised property %s", p.name);
                    }
                }
            }
        }

        protected override BehaviorTask createTask()
        {
            QueryTask pTask = new QueryTask();

            return pTask;
        }

        protected string m_domain;

[Hotfix]
        class Descriptor_t
        {
            public Property Attribute;
            public Property Reference;
            public float Weight;

            public Descriptor_t()
            {
                Attribute = null;
                Reference = null;
                Weight = 0.0f;
            }

            public Descriptor_t(Descriptor_t copy)
            {
                Attribute = copy.Attribute.clone();
                Reference = copy.Reference.clone();
                Weight = copy.Weight;
            }

            ~Descriptor_t()
            {
                Attribute = null;
                Reference = null;
            }
        };

        List<Descriptor_t> m_descriptors;

        static Property FindProperty(Descriptor_t q, List<BehaviorTree.Descriptor_t> c)
        {
            //BehaviorTree.Descriptor_t descriptor = c.Find(delegate (BehaviorTree.Descriptor_t it) { return it.Descriptor.GetVariableId() == q.Attribute.GetVariableId(); });
			for (int i = 0; i < c.Count; ++i)
			{
				BehaviorTree.Descriptor_t descriptor = c[i];
				if (descriptor.Descriptor.GetVariableId() == q.Attribute.GetVariableId())
				{
					return descriptor.Descriptor;
				}
			}

            return null;
        }

        List<Descriptor_t> GetDescriptors()
        {
            return this.m_descriptors;
        }

        protected void SetDescriptors(string descriptors)
        {
            this.m_descriptors = (List<Descriptor_t>)StringUtils.FromString(typeof(List<Descriptor_t>), descriptors, false);
            if (this.m_descriptors == null) return;
            for (int i = 0; i < this.m_descriptors.Count; ++i)
            {
                Descriptor_t d = this.m_descriptors[i];
                d.Attribute.SetDefaultValue(d.Reference);
            }
        }

        float ComputeSimilarity(List<Descriptor_t> q, List<BehaviorTree.Descriptor_t> c)
        {
            float similarity = 0.0f;
            for (int i = 0; i < q.Count; ++i)
            {
                Descriptor_t qi = q[i];

                Property ci = FindProperty(qi, c);

                if (ci != null)
                {
                    float dp = qi.Attribute.DifferencePercentage(ci);
					Debug.Check(dp >= 0.0f && dp <= 1.0f, "dp should be normalized to [0, 1], please check its scale");

                    similarity += (1.0f - dp) * qi.Weight;
                }
            }

            return similarity;
        }

[Hotfix]
        class QueryTask : SingeChildTask
        {
            public QueryTask()
            {
			}

            public override void Init(BehaviorNode node)
            {
                base.Init(node);
            }

            ~QueryTask()
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

			protected override bool isContinueTicking()
			{
				return true;
			}
			
            protected override bool onenter(Agent pAgent)
            {
                //this.m_root = null;

                if (this.ReQuery(pAgent))
                {
                    return true;
                }

                return false;
            }

            protected override void onexit(Agent pAgent, EBTStatus s)
            { }

			public override bool CheckPredicates(Agent pAgent)
			{
				//when there are no predicates, not triggered
				bool bTriggered = false;
				if (this.m_attachments != null)
				{
					bTriggered = base.CheckPredicates(pAgent);
				}
				
				if (bTriggered)
				{
					this.ReQuery(pAgent);
				}
				
				return bTriggered;
			}
			
			bool ReQuery(Agent pAgent)
			{
				Query pQueryNode = this.GetNode() as Query;
				
				if (pQueryNode != null)
				{
					List<Query.Descriptor_t> qd = pQueryNode.GetDescriptors();
					if (qd.Count > 0)
					{
						Dictionary<string, BehaviorTree> bs = Workspace.GetBehaviorTrees();
						
						BehaviorTree btFound = null;
						float similarityMax = -1.0f;
						
						foreach (BehaviorTree bt in bs.Values)
						{
							string domains = bt.GetDomains();
							
							if (string.IsNullOrEmpty(pQueryNode.m_domain) || (!string.IsNullOrEmpty(domains) && domains.IndexOf(pQueryNode.m_domain) != -1))
							{
								List<BehaviorTree.Descriptor_t> bd = bt.GetDescriptors();
								float similarity = pQueryNode.ComputeSimilarity(qd, bd);
								
								if (similarity > similarityMax)
								{
									similarityMax = similarity;
									btFound = bt;
								}
							}
						}
						
						if (btFound != null)
						{
							pAgent.btreferencetree(btFound.GetName());
							//pAgent.btexec();
							
							return true;
						}
					}
				}
				
				return false;
			}

			protected override EBTStatus update(Agent pAgent, EBTStatus childStatus)
			{
				Debug.Check(m_returnStatus == EBTStatus.BT_INVALID);
				///*bool bReQuery = */this.CheckPredicates(pAgent);

				//EBTStatus status = base.update(pAgent, childStatus);
				
				//return status;
				return EBTStatus.BT_RUNNING;
			}
		}
	}
}

