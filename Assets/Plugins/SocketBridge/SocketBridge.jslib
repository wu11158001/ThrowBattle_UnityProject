mergeInto(LibraryManager.library, {

    // Socket 開始連線
    ConnectToSocketJS: function (url) {
        var connectionUrl = UTF8ToString(url);
        
        if (typeof io === 'undefined') {
            console.error("[JS Bridge] 找不到 Socket.IO 函式庫！請檢查 index.html 是否有引入。");
            return;
        }

        console.log("[JS Bridge] 正在連線至: " + connectionUrl);

        window.webglSocket = io(connectionUrl, {
            path: "/socket.io/",
            transports: ['websocket']
        });

        // Socket 連線成功
        window.webglSocket.on('connect', function () {
            console.log("[JS Bridge] Socket 連線成功！");
            SendMessage('SocketManager', 'OnSocketConnectedJS');
        });

        // 配對成功
        window.webglSocket.on('match_success', function (data) {
            console.log("[JS Bridge] 收到配對成功！", data);
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnMatchSuccessJS', jsonStr);
        });

        // 監聽:角色移動
        window.webglSocket.on('on_peer_move_synced', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnPeerMoveSyncedJS', jsonStr);
        });

        // 監聽:畜力狀態
        window.webglSocket.on('on_peer_charging_synced', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnPeerChargingSyncedJS', jsonStr);
        });

        // 監聽:執行投擲
        window.webglSocket.on('on_peer_execute_throw', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnPeerExecuteThrowJS', jsonStr);
        });

        // 監聽:執行擊中
        window.webglSocket.on('on_peer_hit_synced', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnPeerExecuteHitJS', jsonStr);
        });

        // 監聽:回合切換
        window.webglSocket.on('new_turn', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnNewTurnJS', jsonStr);
        });

        // 監聽:聊天訊息
        window.webglSocket.on('on_receive_chat', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnReceiveChatJS', jsonStr);
        });

        // 監聽:貼圖訊息
        window.webglSocket.on('on_receive_stick', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnReciveStickJS', jsonStr);
        });

        // 監聽:回合倒數
        window.webglSocket.on('on_turn_countdown', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnReciveTurnCountDownJS', jsonStr);
        });

        // 監聽:遊戲結束
        window.webglSocket.on('game_over', function (data) {
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnGameOverJS', jsonStr);
        });

        // 監聽:Socket 連線斷開
        window.webglSocket.on('disconnect', function () {
            console.log("[JS Bridge] Socket 連線斷開");
        });
    },

    // 發送 Join 事件
    EmitJoinEventJS: function (playerId) {
        var id = UTF8ToString(playerId);
        if (window.webglSocket) {
            window.webglSocket.emit('join', { playerId: id });
            console.log("[JS Bridge] 已發送 join 事件: " + id);
        }
    },

    // 發送:加入戰鬥房間
    EmitJoinBattleRoomJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('join_battle_room', JSON.parse(rawJson));
        }
    },

    // 發送:角色移動
    EmitSyncMoveJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('sync_move', JSON.parse(rawJson));
        }
    },

    // 發送:開啟閃避
    EmitOpenDodgeJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('open_dodge', JSON.parse(rawJson));
        }
    },

    // 發送:畜力狀態
    EmitSyncChargingJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('sync_charging', JSON.parse(rawJson));
        }
    },

    // 發送:執行投擲
    EmitExecuteThrowJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('execute_throw', JSON.parse(rawJson));
        }
    },

    // 發送:擊中
    EmitExecuteHitJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('execute_hit', JSON.parse(rawJson));
        }
    },

    // 發送:回合結束
    EmitTurnEndJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('turn_end', JSON.parse(rawJson));
        }
    },

    // 發送:聊天訊息
    EmitSendChatJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('send_chat', JSON.parse(rawJson));
        }
    },

    // 發送:貼圖訊息
    EmitSendStickJS: function (jsonStr) {
        var rawJson = UTF8ToString(jsonStr);
        if (window.webglSocket) {
            window.webglSocket.emit('send_stick', JSON.parse(rawJson));
        }
    },
});