<?php

/*
-------------------------------------------------------------------------
get-stroke-orders.php
-------------------------------------------------------------------------

Version 1.1

Originally created by http://en.wikipedia.org/wiki/User_talk:WikiLaurent.
Modified by Gabriel Hooks, 2020-06-10

The original PHP script was found on https://commons.wikimedia.org/wiki/User:WikiLaurent/ScrapCCAnimScript.
However, the original script is almost a decade old, so I have modified it to work properly with the 2020 WikiMedia website.
I have also added some convenient features to the code like downloading the files to separate directories and downloading alternative images if the target 
image is null.

This program is free software you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTYwithout even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

-------------------------------------------------------------------------

Dependency:

Simple HTML DOM Parser (http://simplehtmldom.sourceforge.net/)

-------------------------------------------------------------------------

Usage:

php get-stroke-orders.php n=<page number> t=<animation type>

<page number> - The page number (1 to 7) at http://commons.wikimedia.org/wiki/Commons:Stroke_Order_Project/Simplified_Chinese_progress
<animation type> - "bw", "red" or "order"

-------------------------------------------------------------------------

Example:

Get all the gif animations and bw images if the animation is missing:

php get-stroke-orders.php n=1 t=order
php get-stroke-orders.php n=2 t=order
php get-stroke-orders.php n=3 t=order
php get-stroke-orders.php n=4 t=order
php get-stroke-orders.php n=5 t=order
php get-stroke-orders.php n=6 t=order
php get-stroke-orders.php n=7 t=order

-------------------------------------------------------------------------
*/

require_once "../source/simple_html_dom.php";
set_time_limit(3600 * 10);

function curl($url){
	$ch = curl_init($url);
	curl_setopt($ch, CURLOPT_URL,$url);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER,1);
	curl_setopt($ch, CURLOPT_USERAGENT, "StrokeOrderAnimScrapper/1.0");
	$output = curl_exec($ch);
	curl_close ($ch);
	return $output;
}


function downloadAnimations($pageNumber, $type = "bw") {
	// Get site info
	$listBaseUrl = "https://commons.wikimedia.org/wiki/Commons:Stroke_Order_Project/Simplified_Chinese_progress";
	$pageUrl = $listBaseUrl;
	if ($pageNumber > 1) $pageUrl .= "/" . $pageNumber;
	
	echo "Parsing " . $pageUrl . "\n";
	$hmlString = curl($pageUrl);

	$html = new simple_html_dom();

	$html->load($hmlString);

	// Create directory for the images to go.
	$dir = "../assets/" . $pageNumber;
	mkdir($dir);
	chdir($dir);
	echo "\nCreated Directory " . $dir . "\n";

	// Get the image from each row in the table.
	foreach ($html->find('tr') as $tr) {
		$type = "order";
		$tdIndex = 3;
		if ($type == "red") $tdIndex = 4;
		if ($type == "order") $tdIndex = 5;
		
		$td = $tr->find("td", $tdIndex);
		if (!$td) continue;
		$img = $td->find("img", 0);
		
		// If there isn't an animated stroke order, resort to the diagram image.
		if (!$img) {
			echo "\nCOULD NOT FIND ORDER IMAGE\n";
			$type = "bw";
			$td = $tr->find("td", 3);
			$img = $td->find("img", 0);
			if (!$img) {
				continue;
				echo "\nCOULD NOT FIND BW IMAGE. NO IMAGE WILL BE DOWNLOADED.\n";
			}
		}

		// Idk what this is doing
		$src = $img->getAttribute("src");
		if ($type == "bw" && strpos($src, "-bw.png") === false) continue;
		if ($type == "red" && strpos($src, "-red.png") === false) continue;
		if ($type == "order" && strpos($src, "-order.gif") === false) continue;
		$lastSlashIndex = strrpos($src, "/");
		$src = substr($src, 0, $lastSlashIndex);
		$src = str_replace("/thumb", "", $src);
		
		// Name it
		$alt = $img->getAttribute("alt");
		if ($type == "bw") $filename = substr($alt, 0) . "-bw" . ".png";
		if ($type == "red") $filename = substr($alt, 0) . "-red" . ".png";
		if ($type == "order") $filename = substr($alt, 0) . "-order" . ".gif";
		
		// Download it
		echo "Downloading " . substr($alt, 0) . "\n";
		$pngData = file_get_contents($src);
		file_put_contents($filename, $pngData);
	}
}


function getParam($name) {
	if (isset($_GET[$name])) return $_GET[$name];
	global $argv;
	foreach ($argv as $value) {;
		$pair = explode("=", $value);
		if (count($pair) < 2) continue;
		if (trim($pair[0]) != $name) continue;
		$equalPos = strpos($value, "=");
		return trim(substr($value, $equalPos + 1, strlen($value)));
	}
	return null;
}

downloadAnimations(getParam("n"), getParam("t"));