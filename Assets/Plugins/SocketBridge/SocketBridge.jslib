mergeInto(LibraryManager.library, {

    // 開始連線
    ConnectToSocketJS: function (url) {
        var connectionUrl = UTF8ToString(url);
        
        // 確保網頁有載入 socket.io.js
        if (typeof io === 'undefined') {
            console.error("[JS Bridge] 找不到 Socket.IO 函式庫！請檢查 index.html 是否有引入。");
            return;
        }

        console.log("[JS Bridge] 正在連線至: " + connectionUrl);

        // 建立連線
        window.webglSocket = io(connectionUrl, {
            path: "/socket.io/",
            transports: ['websocket'] // 強制使用 WebSocket
        });

        // 監聽連線成功
        window.webglSocket.on('connect', function () {
            console.log("[JS Bridge] Socket 連線成功！");
            // 呼叫 Unity 內名為 "SocketManager" 的 GameObject 上的 "OnSocketConnectedJS" 函數
            SendMessage('SocketManager', 'OnSocketConnectedJS');
        });

        // 監聽配對成功
        window.webglSocket.on('match_success', function (data) {
            console.log("[JS Bridge] 收到配對成功！", data);
            var jsonStr = typeof data === 'string' ? data : JSON.stringify(data);
            SendMessage('SocketManager', 'OnMatchSuccessJS', jsonStr);
        });

        // 監聽斷線
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
    }
});