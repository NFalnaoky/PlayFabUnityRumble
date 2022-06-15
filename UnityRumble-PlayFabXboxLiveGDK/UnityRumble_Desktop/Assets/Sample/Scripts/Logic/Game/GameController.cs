﻿//--------------------------------------------------------------------------------------
// GameController.cs
//
// The high level game controller that handles game events and manages game objects.
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//
// 
// Copyright (C) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public partial class GameController : MonoBehaviour
{

    [SerializeField]
    private GameAssetManager AssetManager;

    [SerializeField]
    private InputSystemUIInputModule InputModule;

    [SerializeField]
    private Camera GameCamera;

    [SerializeField]
    private StarfieldBehavior Starfield;

    [SerializeField]
    private SpawnController[] ShipSpawners;

    [SerializeField]
    private SpawnController[] AsteroidSpawners;

    [SerializeField]
    private GameObject ShipsContainer;

    [SerializeField]
    private GameObject SatellitesContainer;

    [SerializeField]
    private GameObject AsteroidsContainer;

    public event Action OnLocalPlayerRequestQuit;
    // parameters are: destroyed xuid, destroyer xuid
    public event Action<ulong, ulong> OnShipDestroyed;

    public event Action<ulong> OnRemotePlayerDisconnected;

    public int LocalShipIndex { get; private set; }
    public int LocalColorIndex { get; private set; }
    public ShipController LocalPlayerShip { get; private set; }

    // for upload user achievement
    int killCount = 0;
    float startTime;
    int deadCount = 0;

    public void NowIsHost()
    {
        foreach (var asteroid in _asteroidControllers)
        {
            asteroid.IsHost = true;
        }
    }

    public void Initialize(int localShipIndex, int localColorIndex)
    {
        Debug.LogFormat("GameController.Initialize({0}, {1})", localShipIndex, localColorIndex);
        LocalShipIndex = localShipIndex;
        LocalColorIndex = localColorIndex;
        SessionNetwork.Instance.OnNetworkMessage_GameState_Received += HandleGameStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_PlayerGameState_Received += HandlePlayerGameStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_SpawnAsteroidState_Received += HandleSpawnAsteroidStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_SpawnShipState_Received += HandleSpawnShipStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_DestroyShipState_Received += HandleDestroyShipStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_GoodbyeNetwork_Received += HandleGoodbyeNetworkReceived;
        SessionNetwork.Instance.OnRemotePlayerConnectionStatusChanged += HandleRemotePlayerConnectionStatusChanged;
        startTime = Time.realtimeSinceStartup;
    }

    public void Cleanup()
    {
        Debug.LogFormat("GameController.Cleanup()");

        while (0 < _shipControllers.Count)
        {
            DestroyShip(_shipControllers.First().Key);
        }

        while (0 < _asteroidControllers.Count)
        {
            DestroyAsteroid(0);
        }
        _shipControllers.Clear();
        SessionNetwork.Instance.OnNetworkMessage_GameState_Received -= HandleGameStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_PlayerGameState_Received -= HandlePlayerGameStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_SpawnAsteroidState_Received -= HandleSpawnAsteroidStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_SpawnShipState_Received -= HandleSpawnShipStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_DestroyShipState_Received -= HandleDestroyShipStateReceived;
        SessionNetwork.Instance.OnNetworkMessage_GoodbyeNetwork_Received -= HandleGoodbyeNetworkReceived;
        SessionNetwork.Instance.OnRemotePlayerConnectionStatusChanged -= HandleRemotePlayerConnectionStatusChanged;
    }

    public void SpawnLocalShip(ulong id)
    {
        Debug.LogFormat("GameController.SpawnLocalShip({0})", id);

        var randomShipSpawnerIndex = _randomNumberGenerator.Next(0, ShipSpawners.Length - 1);
        var randomRotation = Convert.ToSingle(_randomNumberGenerator.NextDouble()) * 360F;

        var ship = ShipSpawners[randomShipSpawnerIndex].SpawnShip(
            AssetManager,
            LocalShipIndex,
            LocalColorIndex,
            ShipsContainer.transform);
        ship.transform.rotation = Quaternion.Euler(0F, 0F, randomRotation);

        var inputProvider = ship.gameObject.AddComponent<LocalShipInputProvider>();
        inputProvider.Initialize(SessionNetwork.Instance, ship);

        ship.Initialize(id, inputProvider);
        ship.OnShipDestroyed += HandleShipDestroyed;
        ship.OnLocalPlayerRequestQuit += () => { OnLocalPlayerRequestQuit?.Invoke(); };

        var playerInput = ship.gameObject.GetComponent<PlayerInput>();
        playerInput.uiInputModule = InputModule;
        playerInput.camera = GameCamera;

        _shipControllers.Add(id, ship);

        Assert.IsNull(LocalPlayerShip);
        LocalPlayerShip = ship;

        SessionNetwork.Instance.SendMessageToAll(
            new SpawnShipState(
                LocalShipIndex,
                LocalColorIndex,
                ship.transform.position.x,
                ship.transform.position.y,
                randomRotation), false);
    }

    public void SpawnRemoteShip(ulong ownerId, int shipIndex, int colorIndex, float posX, float posY, float rotation)
    {
        if (!_shipControllers.ContainsKey(ownerId))
        {
            Debug.LogFormat("GameController.SpawnRemoteShip({0})", ownerId);

            var ship = ShipSpawners[0].SpawnShip(AssetManager, shipIndex, colorIndex, ShipsContainer.transform);
            ship.transform.position = new Vector2(posX, posY);
            ship.transform.rotation = Quaternion.Euler(0F, 0F, rotation);

            var inputProvider = ship.gameObject.AddComponent<RemoteShipInputProvider>();
            inputProvider.Initialize(SessionNetwork.Instance, ship);

            ship.Initialize(ownerId, inputProvider);
            _shipControllers.Add(ownerId, ship);
        }
    }

    public void SpawnLocalAsteroids(int minAsteroids, int maxAsteroids)
    {
        Debug.LogFormat("GameController.SpawnLocalAsteroids({0}, {1})", minAsteroids, maxAsteroids);

        var randomAsteroidQuantity = _randomNumberGenerator.Next(minAsteroids, maxAsteroids);
        for (var index = 0; index < randomAsteroidQuantity; index++)
        {
            SpawnLocalAsteroid();
        }
    }

    public void SpawnLocalAsteroid()
    {
        Debug.LogFormat("GameController.SpawnLocalAsteroid()");

        var randomAsteroidSpawnerIndex = _randomNumberGenerator.Next(0, AsteroidSpawners.Length - 1);
        var randomRotation = Convert.ToSingle(_randomNumberGenerator.NextDouble()) * 360F;
        var randomAsteroidIndex = _randomNumberGenerator.Next(0, AssetManager.AsteroidPrefabs.Length - 1);

        var asteroid = AsteroidSpawners[randomAsteroidSpawnerIndex].SpawnAsteroid(
            AssetManager,
            randomAsteroidIndex,
            AsteroidsContainer.transform);

        var id = _asteroidControllers.Count();
        asteroid.Initialize(SessionNetwork.Instance, id, true);
        if (!_asteroidControllers.Contains(asteroid))
        {
            _asteroidControllers.Add(asteroid);
        }
        else
        {
            Destroy(asteroid.gameObject);
        }

        SessionNetwork.Instance.SendMessageToAll(
            new SpawnAsteroidState(
                id,
                randomAsteroidIndex,
                asteroid.transform.position.x,
                asteroid.transform.position.y), false);
    }

    private void SpawnRemoteAsteroid(int id, int asteroidIndex, float posX, float posY)
    {
        Debug.LogFormat("GameController.SpawnRemoteAsteroid()");

        var asteroid = AsteroidSpawners[0].SpawnAsteroid(AssetManager, asteroidIndex, AsteroidsContainer.transform);
        asteroid.transform.position = new Vector2(posX, posY);

        asteroid.Initialize(SessionNetwork.Instance, id, false);
        if (!_asteroidControllers.Contains(asteroid))
        {
            _asteroidControllers.Add(asteroid);
        }
        else
        {
            Destroy(asteroid.gameObject);
        }
    }

    private void OnValidate()
    {
        Assert.IsNotNull(AssetManager);
        Assert.IsNotNull(InputModule);
        Assert.IsNotNull(GameCamera);
        Assert.IsNotNull(Starfield);

        Assert.IsTrue(0 < ShipSpawners.Length);

        Assert.IsNotNull(ShipsContainer);
    }

    private void DestroyShip(ulong owningXuid)
    {
        Debug.LogFormat("GameController.__DestroyShip({0})", owningXuid);

        if (!_shipControllers.ContainsKey(owningXuid))
        {
            return;
        }

        var ship = _shipControllers[owningXuid];
        _shipControllers.Remove(owningXuid);

        if (ship == LocalPlayerShip)
        {
            ship.OnShipDestroyed -= HandleShipDestroyed;
            LocalPlayerShip = null;
        }

        ship.gameObject.SetActive(false);
        ship.transform.parent = null;

        Destroy(ship.gameObject);
    }

    private void DestroyAsteroid(int asteroidIndex)
    {
        Debug.LogFormat("GameController.__DestroyAsteroid({0})", asteroidIndex);

        if (_asteroidControllers.Count <= asteroidIndex)
        {
            return;
        }

        var asteroid = _asteroidControllers[asteroidIndex];
        _asteroidControllers.RemoveAt(asteroidIndex);

        asteroid.gameObject.SetActive(false);
        asteroid.transform.parent = null;

        Destroy(asteroid.gameObject);
    }

    private void HandleShipDestroyed(ShipController ship, ulong byWhoXuid)
    {
        Debug.LogFormat("GameController.__HandleShipDestroyed({0})", ship.OwningSessionMemberId);

        var isLocalShip = ship == LocalPlayerShip;
        var shipXuid = ship.OwningSessionMemberId;

        if (isLocalShip)
        {
            ship.MyShipBehavior.Explode();
            DestroyShip(shipXuid);

            SessionNetwork.Instance.SendMessageToAll(new DestroyShipState(shipXuid, byWhoXuid), false);

            OnShipDestroyed?.Invoke(shipXuid, byWhoXuid);
        }
    }

    private void HandleGoodbyeNetworkReceived(ulong xuid)
    {
        DestroyShip(xuid);
    }

    void HandleRemotePlayerConnectionStatusChanged(ulong id)
    {
        if (_shipControllers.ContainsKey(id))
        {
            DestroyShip(id);
            OnRemotePlayerDisconnected?.Invoke(id);
        }
    }

    private void HandleGameStateReceived(ulong senderXuid, GameState gameState)
    {
        // nothing to do here for now...
    }

    private void HandlePlayerGameStateReceived(ulong senderXuid, PlayerGameState playerGameState)
    {
        // nothing to do here for now...
    }

    private void HandleSpawnAsteroidStateReceived(ulong senderXuid, SpawnAsteroidState spawnAsteroidState)
    {
        SpawnRemoteAsteroid(
            spawnAsteroidState.Id,
            spawnAsteroidState.AsteroidIndex,
            spawnAsteroidState.PosX,
            spawnAsteroidState.PosY);
    }

    private void HandleSpawnShipStateReceived(ulong senderXuid, SpawnShipState spawnShipState)
    {
        SpawnRemoteShip(
            senderXuid,
            spawnShipState.ShipIndex,
            spawnShipState.ColorIndex,
            spawnShipState.PosX,
            spawnShipState.PosY,
            spawnShipState.Rotation);
    }

    private void HandleDestroyShipStateReceived(ulong senderXuid, DestroyShipState destroyShipState)
    {
        if (_shipControllers.ContainsKey(destroyShipState.DestroyedXuid))
        {
            var ship = _shipControllers[destroyShipState.DestroyedXuid];
            ship.MyShipBehavior.Explode();
            DestroyShip(destroyShipState.DestroyedXuid);

            OnShipDestroyed?.Invoke(destroyShipState.DestroyedXuid, destroyShipState.DestroyerXuid);
        }

        if (destroyShipState.DestroyerXuid == ProfileManager.Instance.GetUserId())
        {
            if (killCount == 0)
            {
            }
            killCount++;
        }

        if (destroyShipState.DestroyedXuid == ProfileManager.Instance.GetUserId())
        {
            deadCount++;
        }
    }

    private System.Random _randomNumberGenerator = new System.Random();
    // parameters are: AsteroidController
    private readonly List<AsteroidController> _asteroidControllers =
        new List<AsteroidController>();
    // parameters are: xuid, ShipController
    private readonly Dictionary<ulong, ShipController> _shipControllers =
        new Dictionary<ulong, ShipController>();
}
