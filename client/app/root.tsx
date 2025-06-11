import {
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useNavigation,
} from "@remix-run/react";
import type { LinksFunction } from "@remix-run/node";
import { useEffect, useState } from "react";

import "./app.css";

export const links: LinksFunction = () => [
  { rel: "preconnect", href: "https://fonts.googleapis.com" },
  {
    rel: "preconnect",
    href: "https://fonts.gstatic.com",
    crossOrigin: "anonymous",
  },
  {
    rel: "stylesheet",
    href: "https://fonts.googleapis.com/css2?family=Inter:ital,opsz,wght@0,14..32,100..900;1,14..32,100..900&display=swap",
  },
  {
    rel: "stylesheet",
    href: "https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,200..700,0..1,-50..200&icon_names=add_circle,arrow_back,assignment,assignment_ind,assignment_late,assignment_returned,assignment_turned_in,chevron_right,close,dashboard,delete,domain,download,edit,folder,folder_open,group,image,image_not_supported,mail,menu,more_vert,person_add,person_remove,picture_as_pdf,settings,visibility",
  }];

function RouteChangeAnnouncement() {
  const navigation = useNavigation();
  const [, setAnnouncement] = useState("");

  useEffect(() => {
    if (navigation.state === "idle") return;

    // Announce route changes for accessibility
    setAnnouncement(`Navigating to ${navigation.location.pathname}`);

    // Add a class to the body for transition effects
    document.body.classList.add("route-changing");

    return () => {
      document.body.classList.remove("route-changing");
    };
  }, [navigation.state, navigation.location]);

  return null;
}

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
      </head>
      <body>
        {children}
        <ScrollRestoration />
        <Scripts />
        <RouteChangeAnnouncement />
      </body>
    </html>
  );
}

export default function App() {
  return <Outlet />;
}
