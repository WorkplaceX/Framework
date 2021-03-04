#!/usr/bin/env node

// npx install script

const fs = require('fs');
var path = require('path');

// For console colors see also: https://stackoverflow.com/questions/9781218/how-to-change-node-jss-console-font-color

// Install begin
console.log("\x1b[32m" + "Install websitedefault..." + "\x1b[0m"); // Color green
console.log("Source=" + __dirname);
console.log("Dest=" + process.cwd());

// Make sure dest directory is empty
if (fs.readdirSync(process.cwd()).length == 0) {
  // Copy files
  copy(__dirname, process.cwd());
  // Install end
  console.log("Start website with:");
  console.log("-npm install");
  console.log("-npm start");
  console.log("-Host is then listening on http://localhost:8080/");
  console.log("-Hot reload is active to live modify file src/index.html");
  console.log("\x1b[32m" + "Install succesfull!" + "\x1b[0m"); // Color green
  console.log();  
} else {
  console.log("\x1b[41m" + "Error: This install directory needs to be empty!" + "\x1b[0m"); // Color red
}

// Copy files recursive
function copy(source, dest) {
  fs.readdirSync(source).forEach((file) => {
    fileSource = path.resolve(source, file);
    fileDest = path.resolve(dest, file);
    // console.log("Source=" + fileSource + " " + "Dest=" + fileDest);
    if (fs.statSync(fileSource).isDirectory()) {
      fs.mkdirSync(fileDest);
      copy(fileSource, fileDest);
    } else {
      fs.copyFileSync(fileSource, fileDest);
	}
  });
}

