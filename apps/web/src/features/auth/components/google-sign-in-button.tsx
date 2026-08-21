import { useEffect, useRef, useState } from "react";

import { getGoogleClientId } from "../providers";
import { Button } from "../../../shared/ui/button";

type GoogleSignInButtonProps = {
  onCredential: (credential: string) => Promise<void>;
  onError: (message: string) => void;
  mode?: "signin" | "signup";
};

type GoogleCredentialResponse = {
  credential?: string;
};

declare global {
  interface Window {
    google?: {
      accounts?: {
        id?: {
          initialize: (options: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
          }) => void;
          renderButton: (
            parent: HTMLElement,
            options: {
              theme?: "outline" | "filled_blue" | "filled_black";
              size?: "large" | "medium" | "small";
              shape?: "rectangular" | "pill" | "circle" | "square";
              text?: "signin_with" | "signup_with" | "continue_with" | "signin";
              width?: number;
            }
          ) => void;
        };
      };
    };
  }
}

let googleScriptPromise: Promise<void> | null = null;

function loadGoogleIdentityScript(): Promise<void> {
  if (window.google?.accounts?.id) {
    return Promise.resolve();
  }

  if (googleScriptPromise) {
    return googleScriptPromise;
  }

  googleScriptPromise = new Promise<void>((resolve, reject) => {
    const existingScript = document.querySelector<HTMLScriptElement>("script[data-google-identity='true']");
    if (existingScript) {
      existingScript.addEventListener("load", () => resolve(), { once: true });
      existingScript.addEventListener("error", () => reject(new Error("Unable to load Google Sign-In.")), { once: true });
      return;
    }

    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    script.dataset.googleIdentity = "true";
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("Unable to load Google Sign-In."));
    document.head.appendChild(script);
  });

  return googleScriptPromise;
}

export function GoogleSignInButton({ onCredential, onError, mode = "signin" }: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [isReady, setIsReady] = useState(false);
  const clientId = getGoogleClientId();

  useEffect(() => {
    let isMounted = true;

    async function initializeGoogleButton() {
      if (!clientId) {
        onError("Google sign-in is not configured yet. Add VITE_GOOGLE_CLIENT_ID to the web app.");
        return;
      }

      try {
        await loadGoogleIdentityScript();

        if (!isMounted || !containerRef.current || !window.google?.accounts?.id) {
          return;
        }

        window.google.accounts.id.initialize({
          client_id: clientId,
          callback: (response) => {
            if (!response.credential) {
              onError("Google did not return a sign-in credential.");
              return;
            }

            void onCredential(response.credential);
          }
        });

        containerRef.current.innerHTML = "";
        window.google.accounts.id.renderButton(containerRef.current, {
          theme: "outline",
          size: "large",
          shape: "pill",
          text: mode === "signup" ? "signup_with" : "signin_with",
          width: 320
        });

        setIsReady(true);
      } catch (error) {
        onError(error instanceof Error ? error.message : "Unable to load Google Sign-In.");
      }
    }

    void initializeGoogleButton();

    return () => {
      isMounted = false;
    };
  }, [clientId, mode, onCredential, onError]);

  if (!clientId) {
    return (
      <Button disabled fullWidth variant="secondary">
        Google Sign-In Needs Setup
      </Button>
    );
  }

  return (
    <div
      aria-busy={!isReady}
      className="google-sign-in"
      ref={containerRef}
    />
  );
}
