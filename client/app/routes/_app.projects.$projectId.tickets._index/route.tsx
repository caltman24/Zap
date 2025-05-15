import { LoaderFunctionArgs, redirect } from "@remix-run/node";

// HACK:redirect to the project details page since we dont have a dedicated page for project tickets yet
// We dont need a root route, just this index page since. we dont need breadcrumbs for 'tickets'
export async function loader({ request, params }: LoaderFunctionArgs) {
    const projectId = params.projectId!

    return redirect(`/projects/${projectId}`);
}
