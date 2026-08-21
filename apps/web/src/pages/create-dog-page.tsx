import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

import { createDog } from "../features/dogs/api";
import { CreateDogForm } from "../features/dogs/components/create-dog-form";
import { Button } from "../shared/ui/button";
import { SectionHeader } from "../shared/ui/section-header";

export function CreateDogPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const createDogMutation = useMutation({
    mutationFn: createDog,
    onSuccess: async (dog) => {
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      navigate(`/app/dogs?selected=${dog.id}`);
    }
  });

  return (
    <section className="mooch-page">
      <div className="page-top-actions">
        <Button
          onClick={() => navigate("/app/dogs")}
          type="button"
          variant="secondary"
        >
          Back
        </Button>
      </div>

      <SectionHeader
        description="Create a new dog profile, then we will drop you back into My Dogs with that new dog selected."
        title="Add Dog"
      />

      <section className="panel">
        <CreateDogForm
          ctaLabel="Add best friend"
          helperText="This is the setup that makes the rest of Mooch feel personal: one dog, many dogs, all tracked cleanly."
          isSaving={createDogMutation.isPending}
          onSubmit={createDogMutation.mutateAsync}
        />
      </section>
    </section>
  );
}
