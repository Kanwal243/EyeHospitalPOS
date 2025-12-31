// Barcode Scanner JavaScript Interop
window.BarcodeScanner = {
    // Initialize barcode input focusing
    initializeScannerInput: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },

    // Generate barcode using JsBarcode library (if available)
    generateBarcode: function (barcode, containerId) {
        if (typeof JsBarcode === 'undefined') {
            console.warn('JsBarcode library not loaded');
            return false;
        }

        try {
            JsBarcode(`#${containerId}`, barcode, {
                format: "CODE128",
                width: 2,
                height: 100,
                displayValue: true
            });
            return true;
        } catch (error) {
            console.error('Error generating barcode:', error);
            return false;
        }
    },

    // Generate QR code using QRCode.js library (if available)
    generateQRCode: function (data, containerId, size = 200) {
        const container = document.getElementById(containerId);
        if (!container) return false;

        // Clear previous content
        container.innerHTML = '';

        if (typeof QRCode === 'undefined') {
            console.warn('QRCode library not loaded');
            return false;
        }

        try {
            new QRCode(container, {
                text: data,
                width: size,
                height: size,
                colorDark: "#000000",
                colorLight: "#ffffff"
            });
            return true;
        } catch (error) {
            console.error('Error generating QR code:', error);
            return false;
        }
    },

    // Request camera access for live scanning
    requestCameraAccess: async function (videoElement) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ 
                video: { 
                    facingMode: "environment",
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                } 
            });
            
            if (videoElement && videoElement.srcObject !== stream) {
                videoElement.srcObject = stream;
                await videoElement.play();
            }
            
            return true;
        } catch (error) {
            console.error('Camera access denied:', error);
            return false;
        }
    },

    // Stop camera stream
    stopCameraStream: function (videoElement) {
        if (videoElement && videoElement.srcObject) {
            const stream = videoElement.srcObject;
            const tracks = stream.getTracks();
            tracks.forEach(track => track.stop());
            videoElement.srcObject = null;
        }
    },

    // Capture frame from video and convert to base64
    captureFrame: function (videoElement) {
        if (!videoElement || videoElement.readyState !== videoElement.HAVE_ENOUGH_DATA) {
            return null;
        }

        try {
            const canvas = document.createElement('canvas');
            canvas.width = videoElement.videoWidth;
            canvas.height = videoElement.videoHeight;
            
            const ctx = canvas.getContext('2d');
            ctx.drawImage(videoElement, 0, 0, canvas.width, canvas.height);
            
            // Convert to base64 JPEG (smaller than PNG)
            return canvas.toDataURL('image/jpeg', 0.85);
        } catch (error) {
            console.error('Error capturing frame:', error);
            return null;
        }
    },

    // Initialize barcode detection with frame capture loop
    initBarcodeDetection: function (videoElement, dotNetHelper, scanInterval = 500) {
        if (!videoElement) {
            console.error('Video element not provided');
            return null;
        }

        // Store scanning state in a way that can be cleaned up
        const scanId = 'barcode_scan_' + Date.now();
        const scanState = {
            isScanning: true,
            intervalId: null
        };

        // Store state globally for cleanup
        if (!window._barcodeScanStates) {
            window._barcodeScanStates = {};
        }
        window._barcodeScanStates[scanId] = scanState;

        const scanFrame = async () => {
            if (!scanState.isScanning) return;

            try {
                const imageData = window.BarcodeScanner.captureFrame(videoElement);
                
                if (imageData && dotNetHelper) {
                    // Send image to Blazor for decoding
                    try {
                        await dotNetHelper.invokeMethodAsync('ProcessImageFrame', imageData);
                    } catch (error) {
                        // Silently handle errors (might be throttled on server side)
                        if (error.message && !error.message.includes('throttle')) {
                            console.debug('Scan frame processing:', error.message);
                        }
                    }
                }
            } catch (error) {
                console.error('Error in scan loop:', error);
            }
        };

        // Start scanning loop with setInterval
        scanState.intervalId = setInterval(scanFrame, scanInterval);

        // Return cleanup identifier
        return scanId;
    },

    // Stop barcode detection
    stopBarcodeDetection: function (scanId) {
        if (window._barcodeScanStates && window._barcodeScanStates[scanId]) {
            const scanState = window._barcodeScanStates[scanId];
            scanState.isScanning = false;
            if (scanState.intervalId) {
                clearInterval(scanState.intervalId);
            }
            delete window._barcodeScanStates[scanId];
        }
    },

    // Check if device supports barcode scanning
    isScannerSupported: function () {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    }
};
