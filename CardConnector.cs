using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class CardConnector : Draggable
{
	private enum VisualsState
	{
		None,
		ActiveUnconnected,
		Inactive,
		ActiveConnected
	}

	[HideInInspector]
	public string UniqueId;

	[HideInInspector]
	public GameCard Parent;

	public SpriteRenderer ConnectorIcon;

	[HideInInspector]
	public string ConnectedNodeUniqueId;

	[HideInInspector]
	public CardConnector ConnectedNode;

	public Rectangle ConnectorRect;

	public Rectangle OutlineRect;

	public CardDirection CardDirection;

	public ConnectionType ConnectionType;

	[HideInInspector]
	public Vector3 Middle;

	[HideInInspector]
	public Vector3 MiddleVelo;

	public float ClosedStateScale = 0.2f;

	private bool isActive;

	private Vector3 scaleRef;

	private Vector3 targetScale;

	private Vector3 targetPosition;

	private Vector3 BasePosition;

	private VisualsState currentVisualsState;

	private static List<CardConnector> nodeTracker = new List<CardConnector>();

	public bool IsEnergyConnector
	{
		get
		{
			if (this.ConnectionType != ConnectionType.LV)
			{
				return this.ConnectionType == ConnectionType.HV;
			}
			return true;
		}
	}

	public void InitializeEnergyNode(CardConnectorData data, GameCard parent)
	{
		this.Parent = parent;
		this.CardDirection = data.EnergyConnectionType;
		this.ConnectionType = data.EnergyConnectionStrength;
		this.BasePosition = base.transform.localPosition;
	}

	protected override void Update()
	{
		if (WorldManager.instance.CurrentBoard.Id != "cities")
		{
			bool flag = false;
			if (!WorldManager.instance.CanUseTransport && this.ConnectionType == ConnectionType.Transport)
			{
				flag = true;
			}
			if (this.ConnectionType != ConnectionType.Transport)
			{
				flag = true;
			}
			if (flag)
			{
				base.transform.localScale = Vector3.zero;
				return;
			}
		}
		if (WorldManager.instance.CurrentView == ViewType.Default || WorldManager.instance.CurrentView == ViewType.Calamity)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Energy && this.ConnectionType != ConnectionType.LV && this.ConnectionType != ConnectionType.HV)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Transport && this.ConnectionType != ConnectionType.Transport)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Sewer && this.ConnectionType != ConnectionType.Sewer)
		{
			this.isActive = false;
		}
		else
		{
			this.isActive = true;
		}
		if (this.isActive)
		{
			bool flag2 = InputController.instance.StoppedGrabbing();
			if ((InputController.instance.GetInputEnded(0) || flag2) && CitiesManager.instance.DrawingConnector != null)
			{
				CardConnector cardConnector = WorldManager.instance.HoveredDraggable as CardConnector;
				if (cardConnector != null && cardConnector != CitiesManager.instance.DrawingConnector)
				{
					if (cardConnector.ConnectedNode == null)
					{
						AudioManager.me.PlaySound2D(this.GetConnectSoundForType(this.ConnectionType), 1f, 0.8f);
						CitiesManager.instance.StopDrawCable(WorldManager.instance.HoveredDraggable as CardConnector);
					}
					else
					{
						CitiesManager.instance.DrawingConnector = null;
					}
				}
				else
				{
					CitiesManager.instance.StopDrawCable(null);
				}
			}
		}
		this.UpdateConnectorVisuals();
	}

	private Sprite GetSpriteForConnection(ConnectionType connection)
	{
		return connection switch
		{
			ConnectionType.HV => SpriteManager.instance.HighVoltageSprite, 
			ConnectionType.LV => SpriteManager.instance.LowVoltageSprite, 
			ConnectionType.Sewer => SpriteManager.instance.SewerSprite, 
			ConnectionType.Transport => SpriteManager.instance.TransportSprite, 
			_ => null, 
		};
	}

	private Color GetColorForConnection(ConnectionType connection, bool isConnected)
	{
		switch (connection)
		{
		case ConnectionType.HV:
			if (!isConnected)
			{
				return ColorManager.instance.HighVoltageConnector;
			}
			return ColorManager.instance.HighVoltageConnectorActive;
		case ConnectionType.LV:
			if (!isConnected)
			{
				return ColorManager.instance.LowVoltageConnector;
			}
			return ColorManager.instance.LowVoltageConnectorActive;
		case ConnectionType.Sewer:
			if (!isConnected)
			{
				return ColorManager.instance.SewerConnector;
			}
			return ColorManager.instance.SewerConnectorActive;
		case ConnectionType.Transport:
			if (!isConnected)
			{
				return ColorManager.instance.TransportConnector;
			}
			return ColorManager.instance.TransportConnectorActive;
		default:
			return ColorManager.instance.LowVoltageConnector;
		}
	}

	public void UpdateConnectorVisuals()
	{
		if (!this.Parent.MyBoard.IsCurrent)
		{
			return;
		}
		CardConnector drawingConnector = CitiesManager.instance.DrawingConnector;
		this.ConnectorIcon.sprite = this.GetSpriteForConnection(this.ConnectionType);
		this.targetScale = Vector3.one;
		this.targetPosition = this.BasePosition;
		bool flag = this.ConnectedNode != null;
		if (this.isActive)
		{
			if (!flag && this.currentVisualsState != VisualsState.ActiveUnconnected)
			{
				this.currentVisualsState = VisualsState.ActiveUnconnected;
				this.OutlineRect.Color = Color.black;
				this.ConnectorRect.Color = this.GetColorForConnection(this.ConnectionType, this.ConnectedNode != null);
				SpriteRenderer connectorIcon = this.ConnectorIcon;
				Rectangle connectorRect = this.ConnectorRect;
				int num2 = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Above"));
				int sortingLayerID = (connectorRect.SortingLayerID = num2);
				connectorIcon.sortingLayerID = sortingLayerID;
				Rectangle outlineRect = this.OutlineRect;
				sortingLayerID = (this.ConnectorRect.RenderQueue = 3500);
				outlineRect.RenderQueue = sortingLayerID;
			}
			if (flag && this.currentVisualsState != VisualsState.ActiveConnected)
			{
				this.currentVisualsState = VisualsState.ActiveConnected;
				this.OutlineRect.Color = Color.black;
				this.ConnectorRect.Color = this.GetColorForConnection(this.ConnectionType, this.ConnectedNode != null);
				SpriteRenderer connectorIcon2 = this.ConnectorIcon;
				Rectangle connectorRect2 = this.ConnectorRect;
				int num2 = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Above"));
				int sortingLayerID = (connectorRect2.SortingLayerID = num2);
				connectorIcon2.sortingLayerID = sortingLayerID;
				Rectangle outlineRect2 = this.OutlineRect;
				sortingLayerID = (this.ConnectorRect.RenderQueue = 3500);
				outlineRect2.RenderQueue = sortingLayerID;
			}
			PerformanceHelper.SetActive(this.ConnectorIcon.gameObject, active: true);
			if (WorldManager.instance.HoveredDraggable == this)
			{
				this.targetScale = Vector3.one * 1.1f;
			}
			else
			{
				this.targetScale = Vector3.one;
			}
			if (drawingConnector != null && drawingConnector != this && (drawingConnector.ConnectionType != this.ConnectionType || drawingConnector.CardDirection == this.CardDirection))
			{
				this.targetScale = Vector3.zero;
			}
			if (this.IsHovered)
			{
				if (this.ConnectionType == ConnectionType.LV || this.ConnectionType == ConnectionType.HV)
				{
					string termId = ((this.CardDirection == CardDirection.input) ? "label_connection_type_input" : "label_connection_type_output");
					string termId2 = ((this.ConnectionType == ConnectionType.LV) ? "label_connection_low_voltage" : "label_connection_high_voltage");
					GameScreen.InfoBoxText = SokLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = SokLoc.Translate(termId2) + " " + SokLoc.Translate(termId);
				}
				else if (this.ConnectionType == ConnectionType.Sewer)
				{
					GameScreen.InfoBoxText = SokLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = SokLoc.Translate("label_connection_sewer");
				}
				else if (this.ConnectionType == ConnectionType.Transport)
				{
					string termId3 = ((this.CardDirection == CardDirection.input) ? "label_connection_type_input" : "label_connection_type_output");
					GameScreen.InfoBoxText = SokLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = SokLoc.Translate("label_connection_transport") + " " + SokLoc.Translate(termId3);
				}
			}
		}
		else
		{
			this.SetToBackground();
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.targetScale, 20f * Time.deltaTime);
		base.transform.localPosition = this.targetPosition;
	}

	private void SetToBackground()
	{
		if (this.currentVisualsState != VisualsState.Inactive)
		{
			this.currentVisualsState = VisualsState.Inactive;
			SpriteRenderer connectorIcon = this.ConnectorIcon;
			Rectangle connectorRect = this.ConnectorRect;
			int num2 = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Default"));
			int sortingLayerID = (connectorRect.SortingLayerID = num2);
			connectorIcon.sortingLayerID = sortingLayerID;
			Rectangle outlineRect = this.OutlineRect;
			sortingLayerID = (this.ConnectorRect.RenderQueue = 3000);
			outlineRect.RenderQueue = sortingLayerID;
			this.OutlineRect.Color = WorldManager.instance.CurrentBoard.BoardOptions.CardBackgroundPallete.Color2;
			this.ConnectorRect.Color = WorldManager.instance.CurrentBoard.BoardOptions.CardBackgroundPallete.Color;
		}
		this.targetScale = Vector3.one * 0.75f;
		this.targetPosition = this.BasePosition + Vector3.forward * 0.03f;
		if (Vector3.Distance(base.transform.localScale, this.targetScale) < 0.1f)
		{
			PerformanceHelper.SetActive(this.ConnectorIcon.gameObject, active: false);
		}
	}

	public override void Clicked()
	{
		if (!this.isActive)
		{
			return;
		}
		if (this.ConnectedNode != null)
		{
			if (CitiesManager.instance.DrawingConnector == null)
			{
				this.SetConnectedNode(null);
				CitiesManager.instance.StartDrawCable(this);
			}
			var (clip, vol) = this.GetStartSoundForType(this.ConnectionType);
			AudioManager.me.PlaySound2D(clip, 1f, vol);
		}
		else if (CitiesManager.instance.DrawingConnector == null)
		{
			CitiesManager.instance.StartDrawCable(this);
			var (clip2, vol2) = this.GetStartSoundForType(this.ConnectionType);
			AudioManager.me.PlaySound2D(clip2, 1f, vol2);
		}
	}

	public void SetConnectedNode(CardConnector connector)
	{
		if (connector != null)
		{
			this.ConnectedNode = connector;
			connector.ConnectedNode = this;
			return;
		}
		if (this.ConnectedNode != null)
		{
			this.ConnectedNode.ConnectedNode = null;
		}
		this.ConnectedNode = null;
	}

	public SavedCardConnector ToSavedEnergyConnector()
	{
		if (this.ConnectedNode == null)
		{
			return null;
		}
		return new SavedCardConnector
		{
			UniqueId = this.GetConnectorUniqueId(),
			ConnectedNodeUniqueId = this.ConnectedNode.GetConnectorUniqueId()
		};
	}

	public string GetConnectorUniqueId()
	{
		string uniqueId = this.Parent.CardData.UniqueId;
		string text = this.CardDirection.ToString();
		string text2 = this.ConnectionType.ToString();
		int myIndex = this.GetMyIndex();
		return $"{uniqueId}_{text2}_{text}_{myIndex}";
	}

	private int GetMyIndex()
	{
		int num = 0;
		for (int i = 0; i < this.Parent.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = this.Parent.CardConnectorChildren[i];
			if (cardConnector == this)
			{
				return num;
			}
			if (cardConnector.ConnectionType == this.ConnectionType && cardConnector.CardDirection == this.CardDirection)
			{
				num++;
			}
		}
		throw new Exception();
	}

	public (AudioClip, float) GetStartSoundForType(ConnectionType connection)
	{
		switch (connection)
		{
		case ConnectionType.LV:
		case ConnectionType.HV:
			return (AudioManager.me.EnergyStart, 0.6f);
		case ConnectionType.Sewer:
			return (AudioManager.me.SewerStart, 0.7f);
		case ConnectionType.Transport:
			return (AudioManager.me.TransportStart, 0.8f);
		default:
			return (null, 0f);
		}
	}

	public AudioClip GetConnectSoundForType(ConnectionType connection)
	{
		switch (connection)
		{
		case ConnectionType.LV:
		case ConnectionType.HV:
			return AudioManager.me.EnergyConnected;
		case ConnectionType.Sewer:
			return AudioManager.me.SewerConnected;
		case ConnectionType.Transport:
			return AudioManager.me.TransportConnected;
		default:
			return null;
		}
	}

	public AudioClip GetStretchSoundForType(ConnectionType connection)
	{
		switch (connection)
		{
		case ConnectionType.LV:
		case ConnectionType.HV:
			return AudioManager.me.EnergyStrech;
		case ConnectionType.Sewer:
			return AudioManager.me.SewerStrech;
		case ConnectionType.Transport:
			return AudioManager.me.TransportStrech;
		default:
			return null;
		}
	}

	public bool HasEnergyOutput()
	{
		CardConnector.nodeTracker.Clear();
		return this.Parent.CardData.HasEnergyOutput(this, CardConnector.nodeTracker);
	}

	public bool HasEnergyInput()
	{
		return this.Parent.CardData.HasEnergyInput(this);
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanBeDragged()
	{
		return false;
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		return false;
	}

	protected override void ClampPos()
	{
	}

	public GameCard GetConnectedGameCard()
	{
		return this.ConnectedNode?.Parent;
	}
}
