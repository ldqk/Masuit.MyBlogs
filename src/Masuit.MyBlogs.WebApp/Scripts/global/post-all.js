if (!Detector.webgl) Detector.addGetWebGLMessage();
    var container;
    var camera, scene, renderer;
    var mesh, geometry, material;
    var mouseX = 0, mouseY = 0;
    var start_time = Date.now();
    var windowHalfX = window.innerWidth / 2;
    var windowHalfY = window.innerHeight / 2;
    init();
    function init() {
        container = document.createElement('div');
        container.style.position = "fixed";
        //container.style.backgroundColor = "#ffffff";
        container.style.left = 0;
        container.style.right = 0;
        container.style.bottom = 0;
        container.style.top = 0;
		
        document.body.appendChild(container);
        // Bg gradient
        var canvas = document.createElement('canvas');
        canvas.width = 32;
        canvas.height = window.innerHeight;
        var context = canvas.getContext('2d');
        var gradient = context.createLinearGradient(0, 0, 0, canvas.height);
        gradient.addColorStop(0, "#d0e9ff");
        gradient.addColorStop(0.5, "#4584b4");
        context.fillStyle = gradient;
        context.fillRect(0, 0, canvas.width, canvas.height);
        container.style.background = 'url(' + canvas.toDataURL('image/png') + ')';
        container.style.backgroundSize = '32px 100%';
		container.style.zIndex = -1;
        camera = new THREE.PerspectiveCamera(30, window.innerWidth / window.innerHeight, 1, 3000);
        camera.position.z = 6000;
        scene = new THREE.Scene();
        geometry = new THREE.Geometry();
        var texture = THREE.ImageUtils.loadTexture('cloud10.png', null, animate);
        texture.magFilter = THREE.LinearMipMapLinearFilter;
        texture.minFilter = THREE.LinearMipMapLinearFilter;
        var fog = new THREE.Fog(0x4584b4, - 100, 3000);
        material = new THREE.ShaderMaterial({
            uniforms: {
                "map": { type: "t", value: texture },
                "fogColor": { type: "c", value: fog.color },
                "fogNear": { type: "f", value: fog.near },
                "fogFar": { type: "f", value: fog.far },
            },
            vertexShader: document.getElementById('vs').textContent,
            fragmentShader: document.getElementById('fs').textContent,
            depthWrite: false,
            depthTest: false,
            transparent: true
        });
        var plane = new THREE.Mesh(new THREE.PlaneGeometry(64, 64));
        for (var i = 0; i < 8000; i++) {
            plane.position.x = Math.random() * 1000 - 500;
            plane.position.y = - Math.random() * Math.random() * 200 - 15;
            plane.position.z = i;
            plane.rotation.z = Math.random() * Math.PI;
            plane.scale.x = plane.scale.y = Math.random() * Math.random() * 1.5 + 0.5;
            THREE.GeometryUtils.merge(geometry, plane);
        }
        mesh = new THREE.Mesh(geometry, material);
        scene.add(mesh);
        mesh = new THREE.Mesh(geometry, material);
        mesh.position.z = - 8000;
        scene.add(mesh);
        renderer = new THREE.WebGLRenderer({ antialias: false });
        renderer.setSize(window.innerWidth, window.innerHeight);
        container.appendChild(renderer.domElement);
        document.addEventListener('mousemove', onDocumentMouseMove, false);
        window.addEventListener('resize', onWindowResize, false);
    }
    function onDocumentMouseMove(event) {
        mouseX = (event.clientX - windowHalfX) * 0.25;
        mouseY = (event.clientY - windowHalfY) * 0.15;
    }
    function onWindowResize(event) {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
    }
    function animate() {
        requestAnimationFrame(animate);
        position = ((Date.now() - start_time) * 0.03) % 8000;
        camera.position.x += (mouseX - camera.position.x) * 0.01;
        camera.position.y += (- mouseY - camera.position.y) * 0.01;
        camera.position.z = - position + 8000;
        renderer.render(scene, camera);
    }
$('.choose').click(function () {
	$('.choose').addClass('active');
	$('.choose > .icon').addClass('active');
	$('.pay').removeClass('active');
	$('.wrap').removeClass('active');
	$('.pay > .icon').removeClass('active');
	$('.wrap > .icon').removeClass('active');
	$('#line').addClass('one');
	$('#line').removeClass('two');
	$('#line').removeClass('three');
});
$('.pay').click(function () {
	$('.pay').addClass('active');
	$('.pay > .icon').addClass('active');
	$('.choose').removeClass('active');
	$('.wrap').removeClass('active');
	$('.choose > .icon').removeClass('active');
	$('.wrap > .icon').removeClass('active');
	$('#line').addClass('two');
	$('#line').removeClass('one');
	$('#line').removeClass('three');
});
$('.wrap').click(function () {
	$('.wrap').addClass('active');
	$('.wrap > .icon').addClass('active');
	$('.pay').removeClass('active');
	$('.choose').removeClass('active');
	$('.pay > .icon').removeClass('active');
	$('.choose > .icon').removeClass('active');
	$('#line').addClass('three');
	$('#line').removeClass('two');
	$('#line').removeClass('one');
});
$('.choose').click(function () {
	$('#first').addClass('active');
	$('#second').removeClass('active');
	$('#third').removeClass('active');
});
$('.pay').click(function () {
	$('#first').removeClass('active');
	$('#second').addClass('active');
	$('#third').removeClass('active');
});
$('.wrap').click(function () {
	$('#first').removeClass('active');
	$('#second').removeClass('active');
	$('#third').addClass('active');
});