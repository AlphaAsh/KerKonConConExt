using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContractConfigurator;
using ContractConfigurator.ExpressionParser;
using KerbalKonstructs;
using KerbalKonstructs.LaunchSites;
using UnityEngine;
using KSP;
using KSP.UI.Screens;
using Contracts;

namespace KerKonConConExt
{
	#region Requirements

	public class BaseOpenRequirement : ContractRequirement
	{
		protected string basename { get; set; }

		public override bool LoadFromConfig(ConfigNode configNode)
		{
			bool valid = base.LoadFromConfig(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			return valid;
		}

		public override void OnSave(ConfigNode configNode)
		{
			configNode.AddValue("basename", basename);
		}

		public override void OnLoad(ConfigNode configNode)
		{
			basename = ConfigNodeUtil.ParseValue<String>(configNode, "basename");
		}

		public override bool RequirementMet(ConfiguredContract contract)
		{
			bool bOpen = LaunchSiteManager.getIsSiteOpen(basename);
			if (bOpen) return true;
			else return false;
		}
	}

	public class BaseClosedRequirement : ContractRequirement
	{
		protected string basename { get; set; }

		public override bool LoadFromConfig(ConfigNode configNode)
		{
			bool valid = base.LoadFromConfig(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			return valid;
		}

		public override void OnSave(ConfigNode configNode)
		{
			configNode.AddValue("basename", basename);
		}

		public override void OnLoad(ConfigNode configNode)
		{
			basename = ConfigNodeUtil.ParseValue<String>(configNode, "basename");
		}

		public override bool RequirementMet(ConfiguredContract contract)
		{
			bool bOpen = LaunchSiteManager.getIsSiteOpen(basename);
			if (bOpen) return false;
			else return true;
		}
	}

	public class BaseLockedRequirement : ContractRequirement
	{
		protected string basename { get; set; }

		public override bool LoadFromConfig(ConfigNode configNode)
		{
			bool valid = base.LoadFromConfig(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			return valid;
		}

		public override void OnSave(ConfigNode configNode)
		{
			configNode.AddValue("basename", basename);
		}

		public override void OnLoad(ConfigNode configNode)
		{
			basename = ConfigNodeUtil.ParseValue<String>(configNode, "basename");
		}

		public override bool RequirementMet(ConfiguredContract contract)
		{
			bool bOpen = LaunchSiteManager.getIsSiteLocked(basename);
			if (bOpen) return true;
			else return false;
		}
	}

	public class BaseUnlockedRequirement : ContractRequirement
	{
		protected string basename { get; set; }

		public override bool LoadFromConfig(ConfigNode configNode)
		{
			bool valid = base.LoadFromConfig(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			return valid;
		}

		public override void OnSave(ConfigNode configNode)
		{
			configNode.AddValue("basename", basename);
		}

		public override void OnLoad(ConfigNode configNode)
		{
			basename = ConfigNodeUtil.ParseValue<String>(configNode, "basename");
		}

		public override bool RequirementMet(ConfiguredContract contract)
		{
			bool bOpen = LaunchSiteManager.getIsSiteLocked(basename);
			if (bOpen) return false;
			else return true;
		}
	}

	public class BaseExistsRequirement : ContractRequirement
	{
		protected string basename { get; set; }

		public override bool LoadFromConfig(ConfigNode configNode)
		{
			bool valid = base.LoadFromConfig(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			return valid;
		}

		public override void OnSave(ConfigNode configNode)
		{
			configNode.AddValue("basename", basename);
		}

		public override void OnLoad(ConfigNode configNode)
		{
			basename = ConfigNodeUtil.ParseValue<String>(configNode, "basename");
		}

		public override bool RequirementMet(ConfiguredContract contract)
		{
			bool bOpen = LaunchSiteManager.checkLaunchSiteExists(basename);
			if (bOpen) return true;
			else return false;
		}
	}

	#endregion

	#region ContractBehaviours

	public class CloseBase : ContractBehaviour
	{
		public class ConditionDetail
		{
			public enum Condition
			{
				CONTRACT_ACCEPTED,
				CONTRACT_FAILED,
				CONTRACT_SUCCESS,
				CONTRACT_COMPLETED,
				PARAMETER_FAILED,
				PARAMETER_COMPLETED
			}

			public Condition condition;
			public string parameter;
			public bool disabled = false;
		}

		protected List<ConditionDetail> conditions = new List<ConditionDetail>();
		protected string basename;

		public CloseBase()
            : base()
        {
        }

        public CloseBase(List<ConditionDetail> conditions, string basename)
        {
            this.conditions = conditions;
            this.basename = basename;
        }

		protected override void OnParameterStateChange(ContractParameter param)
		{
			ConditionDetail.Condition cond = param.State == ParameterState.Complete ?
				ConditionDetail.Condition.PARAMETER_COMPLETED :
				ConditionDetail.Condition.PARAMETER_FAILED;
			if (param.State == ParameterState.Incomplete)
			{
				return;
			}
			CloseABase(cond, param.ID, param.State == ParameterState.Complete ? basename : basename);
		}

		protected override void OnAccepted()
		{
			CloseABase(ConditionDetail.Condition.CONTRACT_ACCEPTED, basename);
		}

		protected override void OnCompleted()
		{
			CloseABase(ConditionDetail.Condition.CONTRACT_SUCCESS, basename);
			CloseABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnCancelled()
		{
			CloseABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			CloseABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnDeadlineExpired()
		{
			CloseABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			CloseABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnFailed()
		{
			CloseABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			CloseABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnLoad(ConfigNode configNode)
		{
			foreach (ConfigNode child in configNode.GetNodes("CONDITION"))
			{
				ConditionDetail cd = new ConditionDetail();
				cd.condition = ConfigNodeUtil.ParseValue<ConditionDetail.Condition>(child, "condition");
				cd.parameter = ConfigNodeUtil.ParseValue<string>(child, "parameter", (string)null);
				cd.disabled = ConfigNodeUtil.ParseValue<bool>(child, "disabled");
				conditions.Add(cd);
			}
			basename = ConfigNodeUtil.ParseValue<string>(configNode, "basename");
		}

		protected override void OnSave(ConfigNode configNode)
		{
			foreach (ConditionDetail cd in conditions)
			{
				ConfigNode child = new ConfigNode("CONDITION");
				configNode.AddNode(child);

				child.AddValue("condition", cd.condition);
				if (!string.IsNullOrEmpty(cd.parameter))
				{
					child.AddValue("parameter", cd.parameter);
				}
				child.AddValue("disabled", cd.disabled);
			}
			configNode.AddValue("basename", basename);
		}

		protected void CloseABase(ConditionDetail.Condition condition, string sbasename)
		{
			if (conditions.Any(c => c.condition == condition))
				LaunchSiteManager.setSiteOpenCloseState(sbasename, "Closed");
		}

		protected void CloseABase(ConditionDetail.Condition condition, string parameterID, string sbasename)
		{
			foreach (ConditionDetail cd in conditions.Where(cd => !cd.disabled && cd.condition == condition && cd.parameter == parameterID))
			{
				LaunchSiteManager.setSiteOpenCloseState(sbasename, "Closed");
				cd.disabled = true;
			}
		}
	}

	public class OpenBase : ContractBehaviour
	{
		public class ConditionDetail
		{
			public enum Condition
			{
				CONTRACT_ACCEPTED,
				CONTRACT_FAILED,
				CONTRACT_SUCCESS,
				CONTRACT_COMPLETED,
				PARAMETER_FAILED,
				PARAMETER_COMPLETED
			}

			public Condition condition;
			public string parameter;
			public bool disabled = false;
		}

		protected List<ConditionDetail> conditions = new List<ConditionDetail>();
		protected string basename;

		public OpenBase()
			: base()
		{
		}

		public OpenBase(List<ConditionDetail> conditions, string basename)
		{
			this.conditions = conditions;
			this.basename = basename;
		}

		protected override void OnParameterStateChange(ContractParameter param)
		{
			ConditionDetail.Condition cond = param.State == ParameterState.Complete ?
				ConditionDetail.Condition.PARAMETER_COMPLETED :
				ConditionDetail.Condition.PARAMETER_FAILED;
			if (param.State == ParameterState.Incomplete)
			{
				return;
			}
			OpenABase(cond, param.ID, param.State == ParameterState.Complete ? basename : basename);
		}

		protected override void OnAccepted()
		{
			OpenABase(ConditionDetail.Condition.CONTRACT_ACCEPTED, basename);
		}

		protected override void OnCompleted()
		{
			OpenABase(ConditionDetail.Condition.CONTRACT_SUCCESS, basename);
			OpenABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnCancelled()
		{
			OpenABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			OpenABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnDeadlineExpired()
		{
			OpenABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			OpenABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnFailed()
		{
			OpenABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			OpenABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnLoad(ConfigNode configNode)
		{
			foreach (ConfigNode child in configNode.GetNodes("CONDITION"))
			{
				ConditionDetail cd = new ConditionDetail();
				cd.condition = ConfigNodeUtil.ParseValue<ConditionDetail.Condition>(child, "condition");
				cd.parameter = ConfigNodeUtil.ParseValue<string>(child, "parameter", (string)null);
				cd.disabled = ConfigNodeUtil.ParseValue<bool>(child, "disabled");
				conditions.Add(cd);
			}
			basename = ConfigNodeUtil.ParseValue<string>(configNode, "basename");
		}

		protected override void OnSave(ConfigNode configNode)
		{
			foreach (ConditionDetail cd in conditions)
			{
				ConfigNode child = new ConfigNode("CONDITION");
				configNode.AddNode(child);

				child.AddValue("condition", cd.condition);
				if (!string.IsNullOrEmpty(cd.parameter))
				{
					child.AddValue("parameter", cd.parameter);
				}
				child.AddValue("disabled", cd.disabled);
			}
			configNode.AddValue("basename", basename);
		}

		protected void OpenABase(ConditionDetail.Condition condition, string sbasename)
		{
			if (conditions.Any(c => c.condition == condition))
				LaunchSiteManager.setSiteOpenCloseState(sbasename, "Open");
		}

		protected void OpenABase(ConditionDetail.Condition condition, string parameterID, string sbasename)
		{
			foreach (ConditionDetail cd in conditions.Where(cd => !cd.disabled && cd.condition == condition && cd.parameter == parameterID))
			{
				LaunchSiteManager.setSiteOpenCloseState(sbasename, "Open");
				cd.disabled = true;
			}
		}
	}

	public class LockBase : ContractBehaviour
	{
		public class ConditionDetail
		{
			public enum Condition
			{
				CONTRACT_ACCEPTED,
				CONTRACT_FAILED,
				CONTRACT_SUCCESS,
				CONTRACT_COMPLETED,
				PARAMETER_FAILED,
				PARAMETER_COMPLETED
			}

			public Condition condition;
			public string parameter;
			public bool disabled = false;
		}

		protected List<ConditionDetail> conditions = new List<ConditionDetail>();
		protected string basename;

		public LockBase()
			: base()
		{
		}

		public LockBase(List<ConditionDetail> conditions, string basename)
		{
			this.conditions = conditions;
			this.basename = basename;
		}

		protected override void OnParameterStateChange(ContractParameter param)
		{
			ConditionDetail.Condition cond = param.State == ParameterState.Complete ?
				ConditionDetail.Condition.PARAMETER_COMPLETED :
				ConditionDetail.Condition.PARAMETER_FAILED;
			if (param.State == ParameterState.Incomplete)
			{
				return;
			}
			LockABase(cond, param.ID, param.State == ParameterState.Complete ? basename : basename);
		}

		protected override void OnAccepted()
		{
			LockABase(ConditionDetail.Condition.CONTRACT_ACCEPTED, basename);
		}

		protected override void OnCompleted()
		{
			LockABase(ConditionDetail.Condition.CONTRACT_SUCCESS, basename);
			LockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnCancelled()
		{
			LockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			LockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnDeadlineExpired()
		{
			LockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			LockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnFailed()
		{
			LockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			LockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnLoad(ConfigNode configNode)
		{
			foreach (ConfigNode child in configNode.GetNodes("CONDITION"))
			{
				ConditionDetail cd = new ConditionDetail();
				cd.condition = ConfigNodeUtil.ParseValue<ConditionDetail.Condition>(child, "condition");
				cd.parameter = ConfigNodeUtil.ParseValue<string>(child, "parameter", (string)null);
				cd.disabled = ConfigNodeUtil.ParseValue<bool>(child, "disabled");
				conditions.Add(cd);
			}
			basename = ConfigNodeUtil.ParseValue<string>(configNode, "basename");
		}

		protected override void OnSave(ConfigNode configNode)
		{
			foreach (ConditionDetail cd in conditions)
			{
				ConfigNode child = new ConfigNode("CONDITION");
				configNode.AddNode(child);

				child.AddValue("condition", cd.condition);
				if (!string.IsNullOrEmpty(cd.parameter))
				{
					child.AddValue("parameter", cd.parameter);
				}
				child.AddValue("disabled", cd.disabled);
			}
			configNode.AddValue("basename", basename);
		}

		protected void LockABase(ConditionDetail.Condition condition, string sbasename)
		{
			if (conditions.Any(c => c.condition == condition))
				LaunchSiteManager.setSiteLocked(sbasename);
		}

		protected void LockABase(ConditionDetail.Condition condition, string parameterID, string sbasename)
		{
			foreach (ConditionDetail cd in conditions.Where(cd => !cd.disabled && cd.condition == condition && cd.parameter == parameterID))
			{
				LaunchSiteManager.setSiteLocked(sbasename);
				cd.disabled = true;
			}
		}
	}

	public class UnlockBase : ContractBehaviour
	{
		public class ConditionDetail
		{
			public enum Condition
			{
				CONTRACT_ACCEPTED,
				CONTRACT_FAILED,
				CONTRACT_SUCCESS,
				CONTRACT_COMPLETED,
				PARAMETER_FAILED,
				PARAMETER_COMPLETED
			}

			public Condition condition;
			public string parameter;
			public bool disabled = false;
		}

		protected List<ConditionDetail> conditions = new List<ConditionDetail>();
		protected string basename;

		public UnlockBase()
			: base()
		{
		}

		public UnlockBase(List<ConditionDetail> conditions, string basename)
		{
			this.conditions = conditions;
			this.basename = basename;
		}

		protected override void OnParameterStateChange(ContractParameter param)
		{
			ConditionDetail.Condition cond = param.State == ParameterState.Complete ?
				ConditionDetail.Condition.PARAMETER_COMPLETED :
				ConditionDetail.Condition.PARAMETER_FAILED;
			if (param.State == ParameterState.Incomplete)
			{
				return;
			}
			UnlockABase(cond, param.ID, param.State == ParameterState.Complete ? basename : basename);
		}

		protected override void OnAccepted()
		{
			UnlockABase(ConditionDetail.Condition.CONTRACT_ACCEPTED, basename);
		}

		protected override void OnCompleted()
		{
			UnlockABase(ConditionDetail.Condition.CONTRACT_SUCCESS, basename);
			UnlockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnCancelled()
		{
			UnlockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			UnlockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnDeadlineExpired()
		{
			UnlockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			UnlockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnFailed()
		{
			UnlockABase(ConditionDetail.Condition.CONTRACT_FAILED, basename);
			UnlockABase(ConditionDetail.Condition.CONTRACT_COMPLETED, basename);
		}

		protected override void OnLoad(ConfigNode configNode)
		{
			foreach (ConfigNode child in configNode.GetNodes("CONDITION"))
			{
				ConditionDetail cd = new ConditionDetail();
				cd.condition = ConfigNodeUtil.ParseValue<ConditionDetail.Condition>(child, "condition");
				cd.parameter = ConfigNodeUtil.ParseValue<string>(child, "parameter", (string)null);
				cd.disabled = ConfigNodeUtil.ParseValue<bool>(child, "disabled");
				conditions.Add(cd);
			}
			basename = ConfigNodeUtil.ParseValue<string>(configNode, "basename");
		}

		protected override void OnSave(ConfigNode configNode)
		{
			foreach (ConditionDetail cd in conditions)
			{
				ConfigNode child = new ConfigNode("CONDITION");
				configNode.AddNode(child);

				child.AddValue("condition", cd.condition);
				if (!string.IsNullOrEmpty(cd.parameter))
				{
					child.AddValue("parameter", cd.parameter);
				}
				child.AddValue("disabled", cd.disabled);
			}
			configNode.AddValue("basename", basename);
		}

		protected void UnlockABase(ConditionDetail.Condition condition, string sbasename)
		{
			if (conditions.Any(c => c.condition == condition))
				LaunchSiteManager.setSiteUnlocked(sbasename);
		}

		protected void UnlockABase(ConditionDetail.Condition condition, string parameterID, string sbasename)
		{
			foreach (ConditionDetail cd in conditions.Where(cd => !cd.disabled && cd.condition == condition && cd.parameter == parameterID))
			{
				LaunchSiteManager.setSiteUnlocked(sbasename);
				cd.disabled = true;
			}
		}
	}
	#endregion

	#region BehaviourFactorys

	public class CloseBaseFactory : BehaviourFactory
	{
		protected List<CloseBase.ConditionDetail> conditions = new List<CloseBase.ConditionDetail>();
		protected string basename;

		public override bool Load(ConfigNode configNode)
		{
			// Load base class
			bool valid = base.Load(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			int index = 0;
			foreach (ConfigNode child in ConfigNodeUtil.GetChildNodes(configNode, "CONDITION"))
			{
				DataNode childDataNode = new DataNode("CONDITION_" + index++, dataNode, this);
				try
				{
					ConfigNodeUtil.SetCurrentDataNode(childDataNode);
					CloseBase.ConditionDetail cd = new CloseBase.ConditionDetail();
					valid &= ConfigNodeUtil.ParseValue<CloseBase.ConditionDetail.Condition>(child, "condition", x => cd.condition = x, this);
					valid &= ConfigNodeUtil.ParseValue<string>(child, "parameter", x => cd.parameter = x, this, "", x => ValidateMandatoryParameter(x, cd.condition));
					conditions.Add(cd);
				}
				finally
				{
					ConfigNodeUtil.SetCurrentDataNode(dataNode);
				}
			}
			valid &= ConfigNodeUtil.ValidateMandatoryChild(configNode, "CONDITION", this);

			return valid;
		}

		protected bool ValidateMandatoryParameter(string parameter, CloseBase.ConditionDetail.Condition condition)
		{
			if (parameter == null && (condition == CloseBase.ConditionDetail.Condition.PARAMETER_COMPLETED ||
				condition == CloseBase.ConditionDetail.Condition.PARAMETER_FAILED))
			{
				throw new ArgumentException("Required if condition is PARAMETER_COMPLETED or PARAMETER_FAILED.");
			}
			return true;
		}

		public override ContractBehaviour Generate(ConfiguredContract contract)
		{
			return new CloseBase(conditions, basename);
		}
	}

	public class OpenBaseFactory : BehaviourFactory
	{
		protected List<OpenBase.ConditionDetail> conditions = new List<OpenBase.ConditionDetail>();
		protected string basename;

		public override bool Load(ConfigNode configNode)
		{
			// Load base class
			bool valid = base.Load(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			int index = 0;
			foreach (ConfigNode child in ConfigNodeUtil.GetChildNodes(configNode, "CONDITION"))
			{
				DataNode childDataNode = new DataNode("CONDITION_" + index++, dataNode, this);
				try
				{
					ConfigNodeUtil.SetCurrentDataNode(childDataNode);
					OpenBase.ConditionDetail cd = new OpenBase.ConditionDetail();
					valid &= ConfigNodeUtil.ParseValue<OpenBase.ConditionDetail.Condition>(child, "condition", x => cd.condition = x, this);
					valid &= ConfigNodeUtil.ParseValue<string>(child, "parameter", x => cd.parameter = x, this, "", x => ValidateMandatoryParameter(x, cd.condition));
					conditions.Add(cd);
				}
				finally
				{
					ConfigNodeUtil.SetCurrentDataNode(dataNode);
				}
			}
			valid &= ConfigNodeUtil.ValidateMandatoryChild(configNode, "CONDITION", this);

			return valid;
		}

		protected bool ValidateMandatoryParameter(string parameter, OpenBase.ConditionDetail.Condition condition)
		{
			if (parameter == null && (condition == OpenBase.ConditionDetail.Condition.PARAMETER_COMPLETED ||
				condition == OpenBase.ConditionDetail.Condition.PARAMETER_FAILED))
			{
				throw new ArgumentException("Required if condition is PARAMETER_COMPLETED or PARAMETER_FAILED.");
			}
			return true;
		}

		public override ContractBehaviour Generate(ConfiguredContract contract)
		{
			return new OpenBase(conditions, basename);
		}
	}

	public class LockBaseFactory : BehaviourFactory
	{
		protected List<LockBase.ConditionDetail> conditions = new List<LockBase.ConditionDetail>();
		protected string basename;

		public override bool Load(ConfigNode configNode)
		{
			// Load base class
			bool valid = base.Load(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			int index = 0;
			foreach (ConfigNode child in ConfigNodeUtil.GetChildNodes(configNode, "CONDITION"))
			{
				DataNode childDataNode = new DataNode("CONDITION_" + index++, dataNode, this);
				try
				{
					ConfigNodeUtil.SetCurrentDataNode(childDataNode);
					LockBase.ConditionDetail cd = new LockBase.ConditionDetail();
					valid &= ConfigNodeUtil.ParseValue<LockBase.ConditionDetail.Condition>(child, "condition", x => cd.condition = x, this);
					valid &= ConfigNodeUtil.ParseValue<string>(child, "parameter", x => cd.parameter = x, this, "", x => ValidateMandatoryParameter(x, cd.condition));
					conditions.Add(cd);
				}
				finally
				{
					ConfigNodeUtil.SetCurrentDataNode(dataNode);
				}
			}
			valid &= ConfigNodeUtil.ValidateMandatoryChild(configNode, "CONDITION", this);

			return valid;
		}

		protected bool ValidateMandatoryParameter(string parameter, LockBase.ConditionDetail.Condition condition)
		{
			if (parameter == null && (condition == LockBase.ConditionDetail.Condition.PARAMETER_COMPLETED ||
				condition == LockBase.ConditionDetail.Condition.PARAMETER_FAILED))
			{
				throw new ArgumentException("Required if condition is PARAMETER_COMPLETED or PARAMETER_FAILED.");
			}
			return true;
		}

		public override ContractBehaviour Generate(ConfiguredContract contract)
		{
			return new LockBase(conditions, basename);
		}
	}

	public class UnlockBaseFactory : BehaviourFactory
	{
		protected List<UnlockBase.ConditionDetail> conditions = new List<UnlockBase.ConditionDetail>();
		protected string basename;

		public override bool Load(ConfigNode configNode)
		{
			// Load base class
			bool valid = base.Load(configNode);

			valid &= ConfigNodeUtil.ParseValue<string>(configNode, "basename", x => basename = x, this);

			int index = 0;
			foreach (ConfigNode child in ConfigNodeUtil.GetChildNodes(configNode, "CONDITION"))
			{
				DataNode childDataNode = new DataNode("CONDITION_" + index++, dataNode, this);
				try
				{
					ConfigNodeUtil.SetCurrentDataNode(childDataNode);
					UnlockBase.ConditionDetail cd = new UnlockBase.ConditionDetail();
					valid &= ConfigNodeUtil.ParseValue<UnlockBase.ConditionDetail.Condition>(child, "condition", x => cd.condition = x, this);
					valid &= ConfigNodeUtil.ParseValue<string>(child, "parameter", x => cd.parameter = x, this, "", x => ValidateMandatoryParameter(x, cd.condition));
					conditions.Add(cd);
				}
				finally
				{
					ConfigNodeUtil.SetCurrentDataNode(dataNode);
				}
			}
			valid &= ConfigNodeUtil.ValidateMandatoryChild(configNode, "CONDITION", this);

			return valid;
		}

		protected bool ValidateMandatoryParameter(string parameter, UnlockBase.ConditionDetail.Condition condition)
		{
			if (parameter == null && (condition == UnlockBase.ConditionDetail.Condition.PARAMETER_COMPLETED ||
				condition == UnlockBase.ConditionDetail.Condition.PARAMETER_FAILED))
			{
				throw new ArgumentException("Required if condition is PARAMETER_COMPLETED or PARAMETER_FAILED.");
			}
			return true;
		}

		public override ContractBehaviour Generate(ConfiguredContract contract)
		{
			return new UnlockBase(conditions, basename);
		}
	}
}
#endregion