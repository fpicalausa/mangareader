Manga Reader
===========

Manga Reader is aims to ease the reading of manga on computer screen by allowing easy navigation in a manga page. For this purpose, this application tries to figure out the layout of cells on the manga page, and to use the screen space efficiently to display the various cells. That is, instead of needing to pan, zoom in, and zoom out to be able to read a manga page, this application figures it out on its own.

Usage
--------

The interface is built to be intuitive. First you have to select an image corresponding to a page of the manga from which you want to start reading the manga. Then a few options are provided to determine how the manga should be displayed. After that, the left and right keyboard arrows are used to navigate the manga.

Code organization
-------------------------
The source code is divided into the following projects:

- **MangaParser**: a library that reads image files corresponding to manga pages, and parses them to returns the cells of this page. The reading order of the cells is also determined.  

- MangaParserTest: the unit tests for Manga Parser.

- **VisualMangaParser**: a debugging tool that displays each stage of the parsing process of given manga page. 

- **MangaReader**: the actual manga reading application.
 
The MangaParser library processes the image in various stages, for extensibility purposes; for instance, new algorithms for segmenting an image into cells can be added independently of the algorithms for determining the reading order, and various steps of the existing segmenting process can also be tweaked individually.

TODOs
-----------

The current implementation is at an early stage. Many different points can be improved such as:

- Support for touch gestures
- History of the manga recently read and integration with Windows' jump lists
- Better segmenting when cells are touching.
- Per page parsing settings to allow various algorithms to be used on a same manga

Any contribution is welcome!
