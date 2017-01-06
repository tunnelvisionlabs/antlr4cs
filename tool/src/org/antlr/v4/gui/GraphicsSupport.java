/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.gui;

import javax.imageio.ImageIO;
import javax.print.DocFlavor;
import javax.print.DocPrintJob;
import javax.print.PrintException;
import javax.print.PrintService;
import javax.print.SimpleDoc;
import javax.print.StreamPrintServiceFactory;
import javax.print.attribute.HashPrintRequestAttributeSet;
import javax.print.attribute.PrintRequestAttributeSet;
import javax.swing.*;
import java.awt.*;
import java.awt.image.BufferedImage;
import java.awt.print.PageFormat;
import java.awt.print.Printable;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Arrays;

public class GraphicsSupport {
	public static void saveImage(final JComponent comp, String fileName)
		throws IOException, PrintException
	{
		if ( fileName.endsWith(".ps") || fileName.endsWith(".eps") ) {
			DocFlavor flavor = DocFlavor.SERVICE_FORMATTED.PRINTABLE;
			String mimeType = "application/postscript";
			StreamPrintServiceFactory[] factories =
				StreamPrintServiceFactory.lookupStreamPrintServiceFactories(flavor, mimeType);
			System.out.println(Arrays.toString(factories));
			if (factories.length > 0) {
				FileOutputStream out = new FileOutputStream(fileName);
				PrintService service = factories[0].getPrintService(out);
				SimpleDoc doc = new SimpleDoc(new Printable() {
					@Override
					public int print(Graphics g, PageFormat pf, int page) {
						if (page >= 1) return Printable.NO_SUCH_PAGE;
						else {
							Graphics2D g2 = (Graphics2D) g;
							g2.translate((pf.getWidth() - pf.getImageableWidth()) / 2,
										 (pf.getHeight() - pf.getImageableHeight()) / 2);
							if ( comp.getWidth() > pf.getImageableWidth() ||
								 comp.getHeight() > pf.getImageableHeight() )
							{
								double sf1 = pf.getImageableWidth() / (comp.getWidth() + 1);
								double sf2 = pf.getImageableHeight() / (comp.getHeight() + 1);
								double s = Math.min(sf1, sf2);
								g2.scale(s, s);
							}

							comp.paint(g);
							return Printable.PAGE_EXISTS;
						}
					}
				}, flavor, null);
				DocPrintJob job = service.createPrintJob();
				PrintRequestAttributeSet attributes = new HashPrintRequestAttributeSet();
				job.print(doc, attributes);
				out.close();
			}
		} else {
			// parrt: works with [image/jpeg, image/png, image/x-png, image/vnd.wap.wbmp, image/bmp, image/gif]
			Rectangle rect = comp.getBounds();
			BufferedImage image = new BufferedImage(rect.width, rect.height,
													BufferedImage.TYPE_INT_RGB);
			Graphics2D g = (Graphics2D) image.getGraphics();
			g.setColor(Color.WHITE);
			g.fill(rect);
//			g.setColor(Color.BLACK);
			comp.paint(g);
			String extension = fileName.substring(fileName.lastIndexOf('.') + 1);
			boolean result = ImageIO.write(image, extension, new File(fileName));
			if ( !result ) {
				System.err.println("Now imager for " + extension);
			}
			g.dispose();
		}
	}
}
